using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System.Text;
using System.Threading;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Reflection;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FFXIVVenues.ApiGateway.Controllers.ArgModels;
using FFXIVVenues.ApiGateway.Helpers;
using FFXIVVenues.ApiGateway.Observability;
using FFXIVVenues.ApiGateway.Security;
using FFXIVVenues.DomainData.Context;
using FFXIVVenues.DomainData.Entities.Metrics;
using FFXIVVenues.DomainData.Mapping;
using FFXIVVenues.VenueModels.Observability;
using Microsoft.AspNetCore.Http;
using Dto = FFXIVVenues.VenueModels;
using Domain = FFXIVVenues.DomainData.Entities.Venues;

namespace FFXIVVenues.ApiGateway.Controllers;

/// <summary>
/// Venue endpoints
/// </summary>
[ApiController]
[Route("venue")]
public class VenueController(
    IAuthorizationManager authorizationManager,
    IMapFactory mapFactory,
    IChangeBroker changeBroker,
    DomainDataContext domainData,
    RollingCache<IEnumerable<Dto.Venue>> cache)
    : ControllerBase, IDisposable
{
    private readonly IMapper _modelMapper = mapFactory.GetModelMapper();
    private readonly IMapper _modelProjector = mapFactory.GetModelProjector();

    /// <summary>
    /// Get all/many venues
    /// </summary>
    /// <remarks>
    /// Get approved venues matching given parameters, if none provided
    /// then get all approved venue.
    /// </remarks>
    /// <param name="queryArgs">Arguments for querying venues.</param>
    /// <returns>A collection of filtered venues.</returns>
    [HttpGet]
    public IEnumerable<VenueModels.Venue> Get([FromQuery] VenueQueryArgs queryArgs)
    {
        var query = domainData.Venues.AsQueryable();
        query = queryArgs.ApplyDomainQueryArgs(query);
        query = query.Where(v => v.Deleted == null);
        query = authorizationManager.Check().Can(Operation.Read, query);
        var dtos = this._modelProjector.ProjectTo<Dto.Venue>(query);
        return queryArgs.ApplyDtoQueryArgs(dtos);
    }

    /// <summary>
    /// Get specific venue by Id
    /// </summary>
    /// <param name="id">The Id of the venue.</param>
    /// <param name="recordView">Whether to record this as a view of the venue by a user in telemetry.</param>
    /// <returns>The venue details if found.</returns>
    [HttpGet("{id}")]
    public ActionResult<VenueModels.Venue> GetById(string id, bool? recordView = true)
    {
        var venue = domainData.Venues.Find(id);
        if (venue == null || venue.Deleted != null || authorizationManager.Check().CanNot(Operation.Read, venue))
            return NotFound();

        if (recordView == null || recordView == true)
        {
            domainData.VenueViews.Add(new VenueView(venue));
            domainData.SaveChanges();
        }

        return this._modelMapper.Map<Dto.Venue>(venue);
    }

    /// <summary>
    /// Create or update a venue
    /// </summary>
    /// <remarks>
    /// This endpoint requires an Authorization Key with Create permission
    /// if the venue does not yet exist, or the Update permission if the
    /// venue already exists.
    /// The target venue must be created by the Authorization Key provided
    /// or the provided Authorization Key must have a scope of 'all'.
    /// If the id on the venue does not match the <paramref name="id"/> given
    /// then the request will fail. 
    /// </remarks>
    /// <param name="id">The Id of the venue.</param>
    /// <param name="venue">The venue object to be updated or created.</param>
    /// <returns>The newly created venue.</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult> Put(string id, [FromBody] VenueModels.Venue venue)
    {
        if (venue.Id != id)
            return BadRequest("Venue ID does not match.");

        var existingVenue = domainData.Venues.Include(v => v.Schedule).FirstOrDefault(v => v.Id == id);

        if (existingVenue == null)
        {
            if (authorizationManager.Check().CanNot(Operation.Create))
                return Unauthorized();

            var owningKey = authorizationManager.GetKeyString();
            var newInternalVenue = this._modelMapper.Map<Domain.Venue>(venue);
            newInternalVenue.ScopeKey = owningKey;
            domainData.Venues.Add(newInternalVenue);
            await domainData.SaveChangesAsync();

            changeBroker.Queue(ObservableOperation.Create, newInternalVenue);
            cache.Clear();

            return Ok(this._modelMapper.Map<VenueModels.Venue>(newInternalVenue));
        }

        if (authorizationManager.Check().CanNot(Operation.Update, existingVenue))
            return Unauthorized();

        if (existingVenue.Deleted != null)
            return Unauthorized("Cannot PUT to a deleted venue.");

        this._modelMapper.Map(venue, existingVenue);
        existingVenue.LastModified = DateTimeOffset.UtcNow;
        domainData.Venues.Update(existingVenue);
        await domainData.SaveChangesAsync();

        changeBroker.Queue(ObservableOperation.Update, existingVenue);
        cache.Clear();

        return Ok(this._modelMapper.Map<Dto.Venue>(existingVenue));
    }

    /// <summary>
    /// Delete a venue
    /// </summary>
    /// <remarks>
    /// This endpoint requires an Authorization Key with Delete permission.
    /// The target venue must be created by the Authorization Key provided
    /// or the provided Authorization Key must have a scope of 'all'.
    /// </remarks>
    /// <param name="id">The Id of the venue.</param>
    /// <returns>The deleted venue if successful.</returns>
    [HttpDelete("{id}")]
    public ActionResult<VenueModels.Venue> Delete(string id)
    {
        var venue = domainData.Venues.Find(id);
        if (venue is null || venue.Deleted is not null)
            return NotFound();
        if (authorizationManager.Check().CanNot(Operation.Delete, venue))
            return Unauthorized();

        venue.Deleted = DateTimeOffset.UtcNow;
        domainData.Venues.Update(venue);
        domainData.SaveChanges();

        changeBroker.Queue(ObservableOperation.Delete, venue);
        cache.Clear();

        return Ok(this._modelMapper.Map<Dto.Venue>(venue));
    }

    /// <summary>
    /// Get approval status of a venue
    /// </summary>
    /// <param name="id">The Id of the venue.</param>
    /// <returns>The approval status of the venue if found.</returns>
    [HttpGet("{id}/approved")]
    public ActionResult Approved(string id)
    {
        var venue = domainData.Venues.Find(id);
        if (venue == null || venue.Deleted != null)
            return NotFound();

        if (authorizationManager.Check().CanNot(Operation.Approve, venue))
            return Unauthorized();

        cache.Clear();
        return Ok(venue.Approved);
    }

    /// <summary>
    /// Update approval status of a venue
    /// </summary>
    /// <remarks>
    /// This endpoint requires an Authorization Key with Approve permission.
    /// The target venue must be created by the Authorization Key provided
    /// or the provided Authorization Key must have a scope of 'all'.
    /// </remarks>
    /// <param name="id">The Id of the venue.</param>
    /// <param name="approved">The approval status to be set.</param>
    /// <returns>The new approval status for the venue.</returns>
    [HttpPut("{id}/approved")]
    public async Task<ActionResult> Approved(string id, [FromBody] bool approved)
    {
        var venue = await domainData.Venues.FindAsync(id);
        if (venue == null || venue.Deleted != null)
            return NotFound();

        if (authorizationManager.Check().CanNot(Operation.Approve, venue))
            return Unauthorized();

        if (venue.Approved != approved)
        {
            venue.Approved = approved;
            domainData.Venues.Update(venue);
            await domainData.SaveChangesAsync();

            cache.Clear();
            changeBroker.Queue(ObservableOperation.Update, venue);
        }

        return Ok(venue.Approved);
    }

    private static PropertyInfo _addedField = typeof(Domain.Venue).GetProperty("Added");

    /// <summary>
    /// Update added date of a venue
    /// </summary>
    /// <remarks>
    /// This endpoint requires an Authorization Key with Approve permission.
    /// The target venue must be created by the Authorization Key provided
    /// or the provided Authorization Key must have a scope of 'all'.
    /// </remarks>
    /// <param name="id">The Id of the venue.</param>
    /// <param name="added">The added date to be set.</param>
    /// <returns>A new Added date for the venue.</returns>
    [HttpPut("{id}/added")]
    public ActionResult Added(string id, [FromBody] DateTime added)
    {
        var venue = domainData.Venues.Find(id);
        if (venue == null || venue.Deleted != null)
            return NotFound();

        if (authorizationManager.Check().CanNot(Operation.Approve, venue))
            return Unauthorized();

        venue.Added = new DateTimeOffset(added.ToUniversalTime());
        domainData.Venues.Update(venue);
        domainData.SaveChanges();

        changeBroker.Queue(ObservableOperation.Update, venue);
        cache.Clear();
        return Ok(venue.Added);
    }

    /// <summary>
    /// Update last modified date of a venue
    /// </summary>
    /// <remarks>
    /// This endpoint requires an Authorization Key with Approve permission.
    /// The target venue must be created by the Authorization Key provided
    /// or the provided Authorization Key must have a scope of 'all'.
    /// </remarks>
    /// <param name="id">The Id of the venue.</param>
    /// <param name="lastModified">The last modified date to be set.</param>
    /// <returns>The new Last Modified date for the venue.</returns>
    [HttpPut("{id}/lastmodified")]
    public ActionResult LastModified(string id, [FromBody] DateTime lastModified)
    {
        var venue = domainData.Venues.Find(id);
        if (venue == null || venue.Deleted != null)
            return NotFound();

        if (authorizationManager.Check().CanNot(Operation.Approve, venue))
            return Unauthorized();

        venue.LastModified = new DateTimeOffset(lastModified.ToUniversalTime());
        domainData.Venues.Update(venue);
        domainData.SaveChanges();

        changeBroker.Queue(ObservableOperation.Update, venue);
        cache.Clear();
        return Ok(venue.LastModified);
    }

    /// <summary>
    /// Add/update a schedule override for a venue
    /// </summary>
    /// <remarks>
    /// This endpoint requires an Authorization Key with Update permission.
    /// The target venue must be created by the Authorization Key provided
    /// or the provided Authorization Key must have a scope of 'all'.
    /// </remarks>
    /// <param name="id">The Id of the venue.</param>
    /// <param name="override">The schedule override details.</param>
    /// <returns>The updated venue if successful.</returns>
    [HttpPut("{id}/scheduleoverride")]
    public ActionResult PutScheduleOveride(string id, [FromBody] Dto.ScheduleOverride @override)
    {
        var venue = domainData.Venues.Find(id);
        if (venue == null || venue.Deleted != null)
            return NotFound();

        if (authorizationManager.Check().CanNot(Operation.Update, venue))
            return Unauthorized();

        if (@override.Open && @override.End > @override.Start.AddHours(7))
            return BadRequest("Cannot open for more than 7 hours.");

        var newOverrides = venue.ScheduleOverrides.Where(o => o.Start > @override.End).ToList();
        var domainScheduleOverride = this._modelMapper.Map<Domain.ScheduleOverride>(@override);
        newOverrides.Add(domainScheduleOverride);
        venue.ScheduleOverrides = newOverrides;

        domainData.Venues.Update(venue);
        domainData.SaveChanges();

        changeBroker.Queue(ObservableOperation.Update, venue);
        cache.Clear();

        return Ok(this._modelMapper.Map<Dto.Venue>(venue));
    }

    /// <summary>
    /// Delete a schedule override for a venue
    /// </summary>
    /// <remarks>
    /// This endpoint requires an Authorization Key with Update permission.
    /// The target venue must be created by the Authorization Key provided
    /// or the provided Authorization Key must have a scope of 'all'.
    /// </remarks>
    /// <param name="id">The Id of the venue.</param>
    /// <param name="from">The start time of the override to be deleted.</param>
    /// <param name="to">The end time of the override to be deleted.</param>
    /// <returns>The updated venue if successful.</returns>
    [HttpDelete("{id}/scheduleoverride")]
    public ActionResult DeleteScheduleOveride(string id, [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to)
    {
        var venue = domainData.Venues.Find(id);
        if (venue == null || venue.Deleted != null)
            return NotFound();

        if (authorizationManager.Check().CanNot(Operation.Update, venue))
            return Unauthorized();

        var newOverrides = new List<Domain.ScheduleOverride>();
        if (from is not null)
            newOverrides.AddRange(venue.ScheduleOverrides.Where(o => o.End < from));
        if (to is not null)
            newOverrides.AddRange(venue.ScheduleOverrides.Where(o => o.Start > to));
        venue.ScheduleOverrides = newOverrides;

        domainData.Venues.Update(venue);
        domainData.SaveChanges();

        changeBroker.Queue(ObservableOperation.Update, venue);
        cache.Clear();

        return Ok(this._modelMapper.Map<Dto.Venue>(venue));
    }

    /// <summary>
    /// Observe venue changes
    /// </summary>
    /// <remarks>
    /// <p>
    /// This is a websocket connection endpoint.
    /// </p>
    /// <p>
    /// Before events will be piped into the websocket connection an Observation Request
    /// must be sent on the connection. Subsequent Observation Requests superseeded previous
    /// requests.
    /// </p>
    /// <p>
    /// A subsequent request is a Json formatted object with a 'OperationCriteria' field,
    /// the value either being the string 'Create', 'Update' or 'Delete'.
    /// </p>
    /// <p>
    /// <code>
    /// { OperationCriteria = "Create" }
    /// </code>
    /// </p>
    /// <p>
    /// <list type="table">
    ///   <listheader>
    ///     <term>operation</term>
    ///     <description>description</description>
    ///   </listheader>
    ///   <item>
    ///     <operation>Create</operation>
    ///     <description>Any venue created after this Observation Request for the lifetime
    /// of the connection will be piped to the client.</description>
    ///   </item>
    ///   <item>
    ///     <operation>Update</operation>
    ///     <description>Any venue that already exists and but updated after this Observation Request
    /// for the lifetime of the connection will be piped to the client.</description>
    ///   </item>
    ///   <item>
    ///     <operation>Delete</operation>
    ///     <description>Any venue deleted after this Observation Request for the lifetime
    /// of the connection will be piped to the client.</description>
    ///   </item>
    /// </list>
    /// </p>
    /// <p>
    /// Venues are piped to the client in a Json formatted object with the following fields:
    /// </p>
    /// <p>
    /// <list type="table">
    ///   <listheader>
    ///     <field>operation</field>
    ///     <description>description</description>
    ///   </listheader>
    ///   <item>
    ///     <field>Operation</field>
    ///     <description>Create or Update or Delete</description>
    ///   </item>
    ///   <item>
    ///     <field>SubjectId</field>
    ///     <description>The id of the venue</description>
    ///   </item>
    ///   <item>
    ///     <field>SubjectName</field>
    ///     <description>The name of the venue</description>
    ///   </item>
    ///   <item>
    ///     <field>Approved</field>
    ///     <description>The approved state of the venue</description>
    ///   </item>
    ///   <item>
    ///     <field>DataCenter</field>
    ///     <description>The data center the venue is in</description>
    ///   </item>
    ///   <item>
    ///     <field>World</field>
    ///     <description>The world the venue is in</description>
    ///   </item>
    ///   <item>
    ///     <field>Manager</field>
    ///     <description>The id of the managers on the venue</description>
    ///   </item>
    /// </list>
    /// </p>
    /// </remarks>
    [HttpGet("observe")]
    public async Task Observe()
    {
        if (!this.HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        var webSocket = await this.ControllerContext.HttpContext.WebSockets.AcceptWebSocketAsync();

        Action removeExistingObserver = null;

        var buffer = new byte[1024 * 4];
        while (true)
        {
            WebSocketReceiveResult result = null;
            try
            {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            catch (WebSocketException)
            {
                removeExistingObserver?.Invoke();
                return;
            }

            if (result.CloseStatus.HasValue)
            {
                removeExistingObserver?.Invoke();
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
                break;
            }

            var message = Encoding.UTF8.GetString(new ReadOnlySpan<byte>(buffer, 0, result.Count));
            var observer = JsonSerializer.Deserialize<Observer>(message);
            if (observer == null)
                continue;

            observer.ObserverAction = (op, venue) =>
            {
                var change = Observability.VenueObservation.FromVenue(op, venue);
                var payload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(change));
                return webSocket.SendAsync(payload, WebSocketMessageType.Text, true, CancellationToken.None);
            };
            removeExistingObserver?.Invoke();
            removeExistingObserver = changeBroker.Observe(observer, InvocationKind.Delayed);
        }
    }

    public void Dispose()
    {
        domainData?.Dispose();
    }
}