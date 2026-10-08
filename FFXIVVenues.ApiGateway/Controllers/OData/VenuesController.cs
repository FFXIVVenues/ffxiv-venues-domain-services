using FFXIVVenues.ApiGateway.Helpers;
using FFXIVVenues.ApiGateway.Media;
using FFXIVVenues.ApiGateway.Security;
using FFXIVVenues.DomainData.Context;
using FFXIVVenues.DomainData.Entities.Venues;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using FFXIVVenues.VenueService.Client.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Wolverine;

namespace FFXIVVenues.ApiGateway.Controllers.OData;

public class VenuesController(
    DomainDataContext db,
    IMessageBus bus,
    ICurrentUser user,
    IMediaRepository media,
    IOptionsSnapshot<MediaConfiguration> mediaConfig) : ODataController
{

    private const long MaxBannerBytes = 10_048_576;

    [EnableQuery]
    [AllowAnonymous]
    public ActionResult<IQueryable<Venue>> Get()
    {
        if (user.IsAuthenticated)
            return Ok(db.Venues.AsNoTracking().Where(v => (v.Approved || v.Managers.Contains(user.Id.ToString())) && v.Deleted == null));
        return Ok(db.Venues.AsNoTracking().Where(v => v.Approved && v.Deleted == null));
    }

    [EnableQuery]
    [AllowAnonymous]
    public async Task<ActionResult<Venue>> Get([FromRoute] string key)
    {
        var venue = await db.Venues.AsNoTracking().SingleOrDefaultAsync(d => d.Id == key);
        if (venue == null)
            return NotFound();
        if (venue.Deleted != null)
            return NotFound();
        if (venue.Approved || venue.Managers?.Contains(user.Id.ToString()) == true)
            return Ok(venue);
        return NotFound();
    }
    
    [EnableQuery]
    public async Task<ActionResult<Venue>> Post([FromBody] Venue venue)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // TODO: Allow overriding this check as Staff
        var venuesCreatedInLast24Hours = await db.Venues.CountAsync(
            v => v.Added >= DateTimeOffset.UtcNow.AddDays(-1) 
              && v.Managers.Contains(user.Id.ToString()));
        if (venuesCreatedInLast24Hours >= 3)
            return Forbid("You have reached the limit of 3 venues created in the last 24 hours.");

        venue.Id = IdHelper.GenerateId();
        venue.Banner = null;
        venue.Approved = false;
        venue.Managers = [ user.Id.ToString() ]; // TODO: Allow overriding this as Staff
        
        // TODO: Allow overriding this check as Staff
        if (venue.Schedule.Any(s => 
                new TimeOnly(s.EndHour!.Value, s.EndMinute!.Value) - new TimeOnly(s.StartHour, s.StartMinute) > TimeSpan.FromHours(7)))
            return BadRequest("Cannot set Schedule to more than 7 hours");
        
        // TODO: Allow overriding this check as Staff
        if (venue.ScheduleOverrides.Any(s => s.End - s.Start > TimeSpan.FromHours(7)))
            return BadRequest("Cannot set Schedule Override to more than 7 hours");
        
        await db.Venues.AddAsync(venue);
        await db.SaveChangesAsync();

        await bus.PublishAsync(new VenueCreatedEvent(venue.Id, user.Id), new DeliveryOptions { ScheduleDelay = TimeSpan.FromSeconds(5) });
        return Created(venue);
    }

    [EnableQuery]
    public async Task<ActionResult<Venue>> Patch([FromRoute] string key, [FromBody] Delta<Venue> venue)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existingVenue = await db.Venues.FindAsync(key);
        if (existingVenue == null || existingVenue.Deleted != null)
            return NotFound();
        
        if (existingVenue.Managers?.Contains(user.Id.ToString()) != true)
            return Forbid();

        await db.LoadChangedNavigationsAsync(existingVenue, venue);
        venue.CopyChangedValues(existingVenue);
        
        // ReSharper disable EntityFramework.NPlusOne.Usage
        // TODO: Allow overriding this check as Staff
        if (venue.GetChangedPropertyNames().Contains(nameof(Venue.Schedule)) &&
            existingVenue.Schedule!.Any(s => 
                new TimeOnly(s.EndHour!.Value, s.EndMinute!.Value) - new TimeOnly(s.StartHour, s.StartMinute) > TimeSpan.FromHours(7)))
            return BadRequest("Cannot set Schedule to more than 7 hours");
        
        // TODO: Allow overriding this check as Staff
        if (venue.GetChangedPropertyNames().Contains(nameof(Venue.ScheduleOverrides)) &&
            existingVenue.ScheduleOverrides.Any(s => s.End - s.Start > TimeSpan.FromHours(7)))
            return BadRequest("Cannot set Schedule Override to more than 7 hours");
        // ReSharper enable EntityFramework.NPlusOne.Usage
        
        await db.SaveChangesAsync();

        await bus.PublishAsync(new VenueUpdatedEvent(existingVenue.Id, user.Id), new DeliveryOptions { ScheduleDelay = TimeSpan.FromSeconds(5) });
        return Ok(existingVenue);
    }

    public async Task<ActionResult<Venue>> Delete([FromRoute] string key)
    {
        var venue = await db.Venues.FindAsync(key);
        if (venue == null || venue.Deleted != null)
            return NotFound();

        if (venue.Managers?.Contains(user.Id.ToString()) != true)
            return Forbid();

        venue.Deleted = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync();
        
        await bus.SendAsync(new VenueDeletedEvent(venue.Id, user.Id), new DeliveryOptions { ScheduleDelay = TimeSpan.FromSeconds(5) });
        return venue;
    }

    [HttpGet("odata/Venues({key})/banner")]
    public async Task<ActionResult> GetBanner([FromRoute] string key)
    {
        if (media.IsMetered)
            return StatusCode(StatusCodes.Status403Forbidden);

        var venue = await db.Venues.AsNoTracking().SingleOrDefaultAsync(v => v.Id == key && v.Approved && v.Deleted == null);
        if (venue == null)
            return NotFound();

        if (string.IsNullOrEmpty(venue.Banner))
            return NoContent();

        var (stream, contentType) = await media.Download(venue.Id, venue.Banner, HttpContext.RequestAborted);
        return File(stream, contentType);
    }

    [HttpPut("odata/Venues({key})/banner")]
    public async Task<ActionResult> PutToBanner([FromRoute] string key)
    {
        var venue = await db.Venues.FindAsync(key);
        if (venue == null || venue.Deleted != null)
            return NotFound();

        if (venue.Managers?.Contains(user.Id.ToString()) != true)
            return Forbid();

        if (Request.ContentLength is not > 0)
            return StatusCode(StatusCodes.Status411LengthRequired);
        if (Request.ContentLength > MaxBannerBytes)
            return StatusCode(StatusCodes.Status413PayloadTooLarge);
        if (Request.ContentType?.StartsWith("image/") != true)
            return StatusCode(StatusCodes.Status415UnsupportedMediaType);

        var previousBanner = venue.Banner;
        venue.Banner = await media.Upload(venue.Id, Request.ContentType, Request.ContentLength.Value, Request.Body, HttpContext.RequestAborted);
        venue.LastModified = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync();

        if (!string.IsNullOrEmpty(previousBanner))
            await media.Delete(venue.Id, previousBanner);

        await bus.PublishAsync(new VenueUpdatedEvent(venue.Id, user.Id), new DeliveryOptions { ScheduleDelay = TimeSpan.FromSeconds(5) });
        
        var bannerUri = new Uri(mediaConfig.Value.MediaUriTemplate.Replace("{venueId}", venue.Id).Replace("{bannerKey}", venue.Banner));
        return this.Created(bannerUri, null);
    }

    [HttpDelete("odata/Venues({key})/banner")]
    public async Task<ActionResult> DeleteToBanner([FromRoute] string key)
    {
        var venue = await db.Venues.FindAsync(key);
        if (venue == null || venue.Deleted != null)
            return NotFound();

        if (venue.Managers?.Contains(user.Id.ToString()) != true)
            return Forbid();

        if (string.IsNullOrEmpty(venue.Banner))
            return NoContent();

        var previousBanner = venue.Banner;
        venue.Banner = null;
        venue.LastModified = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync();

        await media.Delete(venue.Id, previousBanner);

        await bus.PublishAsync(new VenueUpdatedEvent(venue.Id, user.Id), new DeliveryOptions { ScheduleDelay = TimeSpan.FromSeconds(5) });
        return NoContent();
    }

}
