using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FFXIVVenues.ApiGateway.Helpers;
using FFXIVVenues.ApiGateway.Observability;
using FFXIVVenues.ApiGateway.Security;
using FFXIVVenues.DomainData.Context;
using FFXIVVenues.VenueModels.Observability;
using Microsoft.AspNetCore.Mvc;

using Dto = FFXIVVenues.VenueModels;
using Domain = FFXIVVenues.DomainData.Entities.Venues;

namespace FFXIVVenues.ApiGateway.Controllers;

/// <summary>
/// Venue approval endpoints.
/// Venues are not visible to public queries unless they are approved.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("v{apiVersion:ApiVersion}/venue")]
[ApiExplorerSettings(IgnoreApi = true)]
public class MetadataController(
    IAuthorizationManager authorizationManager,
    IChangeBroker changeBroker,
    DomainDataContext domainData,
    RollingCache<IEnumerable<Dto.Venue>> cache)
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

        cache.Clear();
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
    public ActionResult LastModified(string id, [FromBody] DateTime? lastModified)
    {
        var venue = domainData.Venues.Find(id);
        if (venue == null || venue.Deleted != null)
            return NotFound();

        if (authorizationManager.Check().CanNot(Operation.Approve, venue))
            return Unauthorized();

        if (!lastModified.HasValue)
            venue.LastModified = null;
        else 
            venue.LastModified = new DateTimeOffset(lastModified.Value.ToUniversalTime());
        domainData.Venues.Update(venue);
        domainData.SaveChanges();

        changeBroker.Queue(ObservableOperation.Update, venue);
        cache.Clear();
        return Ok(venue.LastModified);
    }
}