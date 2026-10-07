using FFXIVVenues.ApiGateway.Security;
using FFXIVVenues.DomainData.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using FFXIVVenues.VenueService.Client.Events;
using Wolverine;

namespace FFXIVVenues.ApiGateway.Controllers.V1._0;

/// <summary>
/// Venue approval endpoints.
/// Venues are not visible to public queries unless they are approved.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("v{apiVersion:ApiVersion}/venue")]
[ApiExplorerSettings(IgnoreApi = true)]
[AllowAnonymous]
public class MetadataController(
    IAuthorizationManager authorizationManager,
    IMessageBus bus,
    DomainDataContext domainData)
    : ControllerBase
{
    /// <summary>
    /// Get venue approval status
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

        return Ok(venue.Approved);
    }

    /// <summary>
    /// Update venue approval status
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

        if (venue.Approved == approved) 
            return Ok(venue.Approved);
        
        venue.Approved = approved;
        domainData.Venues.Update(venue);
        await domainData.SaveChangesAsync();
        await bus.SendAsync(new VenueUpdatedEvent(venue.Id, 0), new DeliveryOptions { ScheduleDelay = TimeSpan.FromSeconds(5) });

        return Ok(venue.Approved);
    }
    
    /// <summary>
    /// Update venue added date 
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
    public async Task<ActionResult> Added(string id, [FromBody] DateTime added)
    {
        var venue = domainData.Venues.Find(id);
        if (venue == null || venue.Deleted != null)
            return NotFound();

        if (authorizationManager.Check().CanNot(Operation.Approve, venue))
            return Unauthorized();

        venue.Added = new DateTimeOffset(added.ToUniversalTime());
        domainData.Venues.Update(venue);
        domainData.SaveChanges();

        await bus.SendAsync(new VenueUpdatedEvent(venue.Id, 0), new DeliveryOptions { ScheduleDelay = TimeSpan.FromSeconds(5) });
        return Ok(venue.Added);
    }

    /// <summary>
    /// Update venue last modified date
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
    public async Task<ActionResult> LastModified(string id, [FromBody] DateTime? lastModified)
    {
        var venue = await domainData.Venues.FindAsync(id);
        if (venue == null || venue.Deleted != null)
            return NotFound();

        if (authorizationManager.Check().CanNot(Operation.Approve, venue))
            return Unauthorized();

        if (!lastModified.HasValue)
            venue.LastModified = null;
        else 
            venue.LastModified = new DateTimeOffset(lastModified.Value.ToUniversalTime());
        domainData.Venues.Update(venue);
        await domainData.SaveChangesAsync();

        await bus.SendAsync(new VenueUpdatedEvent(venue.Id, 0), new DeliveryOptions { ScheduleDelay = TimeSpan.FromSeconds(5) });
        return Ok(venue.LastModified);
    }
}