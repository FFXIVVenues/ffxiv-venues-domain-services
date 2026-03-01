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
/// Venue querying and authoring endpoints
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("v{apiVersion:ApiVersion}/venue")]
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
    /// If the id on the venue does not match the <paramref name="id"/> given
    /// then the request will fail. 
    /// The target venue must be created by the Authorization Key provided
    /// for the target venue.
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
    /// This endpoint requires an Authorization Key with Delete permission
    /// for the target venue.
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
    /// Add/update venue schedule override
    /// </summary>
    /// <remarks>
    /// This endpoint requires an Authorization Key with Update permission.
    /// The target venue must be created by the Authorization Key provided
    /// for the target venue.
    /// </remarks>
    /// <param name="id">The Id of the venue.</param>
    /// <param name="override">The schedule override details.</param>
    /// <returns>The updated venue if successful.</returns>
    [HttpPut("{id}/scheduleoverride")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public ActionResult PutScheduleOveride(string id, [FromBody] Dto.ScheduleOverride @override)
    {
        var venue = domainData.Venues.Find(id);
        if (venue == null || venue.Deleted != null)
            return NotFound();

        if (authorizationManager.Check().CanNot(Operation.Update, venue))
            return Unauthorized();

        // We 1 second here to give grace to millisecond variance
        if (@override.Open && @override.End > @override.Start.AddHours(7).AddSeconds(1))
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
    /// Delete venue schedule override 
    /// </summary>
    /// <remarks>
    /// This endpoint requires an Authorization Key with Update permission.
    /// The target venue must be created by the Authorization Key provided
    /// for the target venue.
    /// </remarks>
    /// <param name="id">The Id of the venue.</param>
    /// <param name="from">The start time of the override to be deleted.</param>
    /// <param name="to">The end time of the override to be deleted.</param>
    /// <returns>The updated venue if successful.</returns>
    [HttpDelete("{id}/scheduleoverride")]
    [ApiExplorerSettings(IgnoreApi = true)]
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
    /// This is a websocket connection endpoint.
    /// 
    /// Before events will be piped into the websocket connection an Observation Request
    /// must be sent on the connection. Subsequent Observation Requests superseed previous
    /// requests.
    /// 
    /// An observation request is a Json formatted object with an 'OperationCriteria' field,
    /// the value being an Operation; being either 'Create', 'Update' or 'Delete'.
    /// 
    /// <code>
    /// { OperationCriteria = "Create" }
    /// </code>
    /// 
    /// ## Operations
    /// **Create**
    /// 
    /// Any venue created after this Observation Request for the lifetime
    /// of the connection will be piped to the client.
    /// 
    /// **Update**
    /// 
    /// Any venue that already exists and updated after this Observation Request
    /// for the lifetime of the connection will be piped to the client.
    /// 
    /// **Delete**
    /// 
    /// Any venue deleted after this Observation Request for the lifetime
    /// of the connection will be piped to the client.
    /// 
    /// ## Response
    /// 
    /// Venues are piped to the client in a Json formatted object with the following fields:
    /// 
    /// **Operation**
    /// 
    /// Create or Update or Delete
    /// 
    /// **SubjectId**
    /// 
    /// The id of the venue
    /// 
    /// **SubjectName**
    /// 
    /// The name of the venue
    /// 
    /// **Approved**
    /// 
    /// The approved state of the venue
    /// 
    /// **DataCenter**
    /// 
    /// The data center the venue is in
    /// 
    /// **World**
    /// 
    /// The world the venue is in
    /// 
    /// **Manager**
    /// 
    /// The id of the managers on the venue
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