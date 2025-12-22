using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using FFXIVVenues.ApiGateway.Helpers;
using FFXIVVenues.ApiGateway.Media;
using FFXIVVenues.ApiGateway.Observability;
using FFXIVVenues.ApiGateway.Security;
using FFXIVVenues.DomainData.Context;
using FFXIVVenues.VenueModels.Observability;
using Microsoft.AspNetCore.Http;

namespace FFXIVVenues.ApiGateway.Controllers;

/// <summary>
/// Venue image endpoints
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("v{apiVersion:ApiVersion}/venue/{id}/media")]
public class MediaController(
    IMediaRepository mediaManager,
    IAuthorizationManager authorizationManager,
    IChangeBroker changeBroker,
    DomainDataContext domainData,
    RollingCache<IEnumerable<VenueModels.Venue>> cache)
    : ControllerBase, IDisposable
{

    /// <summary>
    /// Get a venue's image.
    /// </summary>
    /// <param name="id">The Id of the venue.</param>
    /// <code></code>
    /// <returns>The image for the venue, otherwise a default banner image.</returns>
    [HttpGet]
    public async Task<ActionResult> GetAsync(string id)
    {
        if (mediaManager.IsMetered)
            return this.StatusCode(StatusCodes.Status403Forbidden);
        
        var venue = await domainData.Venues.FindAsync(id);
        if (venue is null || venue.Deleted is not null || authorizationManager.Check().CanNot(Operation.Read, venue))
            return NotFound();

        if (string.IsNullOrEmpty(venue.Banner))
            return new FileStreamResult(System.IO.File.OpenRead("default-banner.jpg"), "image/jpeg");

        var (stream, contentType) = await mediaManager.Download(venue.Id, venue.Banner, HttpContext.RequestAborted);

        return File(stream, contentType);
    }

    /// <summary>
    /// Upload / update the image for a venue
    /// </summary>
    /// <remarks>
    /// This endpoint requires an Authorization Key with Update permission.
    /// The target venue must be created by the Authorization Key provided
    /// or the provided Authorization Key must have a scope of 'all'.
    /// </remarks>
    /// <param name="id">The Id of the venue.</param>
    [HttpPut]
    public async Task<ActionResult> PutAsync(string id)
    {
        var venue = await domainData.Venues.FindAsync(id);

        if (venue is null)
            return NotFound();
        if (authorizationManager.Check().CanNot(Operation.Update, venue))
            return Unauthorized();
        if (venue.Deleted is not null)
            return Unauthorized("Cannot PUT to a deleted venue.");
        if (Request.ContentLength > 10_048_576)
            return BadRequest();
        if (Request.ContentType?.StartsWith("image/") == false)
            return BadRequest();

        if (!string.IsNullOrEmpty(venue.Banner))
            await mediaManager.Delete(venue.Id, venue.Banner);

        if (! Request.ContentLength.HasValue)
            return this.BadRequest();
        
        venue.Banner = await mediaManager.Upload(id, Request.ContentType, Request.ContentLength.Value, Request.Body, HttpContext.RequestAborted);
        venue.LastModified = DateTimeOffset.UtcNow;
        domainData.Venues.Update(venue);
        await domainData.SaveChangesAsync();

        cache.Clear();
        changeBroker.Queue(ObservableOperation.Update, venue);

        return NoContent();
    }

    /// <summary>
    /// Delete the media (banner) associated with a specific venue.
    /// </summary>
    /// <remarks>
    /// This endpoint requires an Authorization Key with Delete permission.
    /// The target venue must be created by the Authorization Key provided
    /// or the provided Authorization Key must have a scope of 'all'.
    /// </remarks>
    /// <param name="id">The Id of the venue.</param>
    [HttpDelete]
    public async Task<ActionResult> Delete(string id)
    {
        var venue = await domainData.Venues.FindAsync(id);
        if (venue is null || venue.Deleted is not null)
            return NotFound();

        if (authorizationManager.Check().CanNot(Operation.Delete, venue))
            return Unauthorized();

        if (string.IsNullOrEmpty(venue.Banner))
            return NoContent();

        await mediaManager.Delete(venue.Id, venue.Banner);

        venue.Banner = null;
        domainData.Venues.Update(venue);
        await domainData.SaveChangesAsync();

        cache.Clear();
        changeBroker.Queue(ObservableOperation.Update, venue);

        return NoContent();
    }

    public void Dispose()
    {
        domainData?.Dispose();
    }
}