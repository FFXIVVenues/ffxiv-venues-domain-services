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
using JasperFx.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.Extensions.Options;
using Wolverine;

namespace FFXIVVenues.ApiGateway.Controllers.OData;

public class VenuesController(
    DomainDataContext db,
    IMessageBus bus,
    ICurrentUser user,
    IMediaRepository media,
    IAuthorizationProvider auth,
    IOptionsSnapshot<MediaConfiguration> mediaConfig) : ODataController
{

    private const long MAX_BANNER_BYTES = 10_048_576;

    [EnableQuery]
    [AllowAnonymous]
    public ActionResult<IQueryable<Venue>> Get()
    {
        // TODO: ensure frontend filters to approved
        // so managers don't confuse their venues as showing on the 
        // site and approved when in fact only they can see it there
        if (user.IsAuthenticated)
        {
            var venues = db.Venues.AsNoTrackingWithIdentityResolution().Where(v => v.Deleted == null);
            var ids = venues.Where(v => v.Approved).Select(v => v.Id);
            ids = ids.Union(venues.Where(v => v.Managers.Contains(user.Id.ToString())).Select(v => v.Id));
            
            if (user.Authorization is { Edit: not null and not Authorization.ODATA_FALSE })
                ids = ids.Union(venues.Where(auth.QueryToExpression<Venue>(user.Authorization.Edit)).Select(v => v.Id));
            if (user.Authorization is { EditApproval: not null and not Authorization.ODATA_FALSE })
                ids = ids.Union(venues.Where(auth.QueryToExpression<Venue>(user.Authorization.EditApproval)).Select(v => v.Id));
            if (user.Authorization is { EditManagers: not null and not Authorization.ODATA_FALSE })
                ids = ids.Union(venues.Where(auth.QueryToExpression<Venue>(user.Authorization.EditManagers)).Select(v => v.Id));
            if (user.Authorization is { Delete: not null and not Authorization.ODATA_FALSE })
                ids = ids.Union(venues.Where(auth.QueryToExpression<Venue>(user.Authorization.Delete)).Select(v => v.Id));
            
            return Ok(db.Venues.AsNoTrackingWithIdentityResolution().Where(v => ids.Contains(v.Id)));
        } 
        return Ok(db.Venues.AsNoTracking().Where(v => v.Approved && v.Deleted == null));
    }

    [EnableQuery]
    [AllowAnonymous]
    public async Task<ActionResult<SingleResult<Venue>>> Get([FromRoute] string key)
    {
        var venue = await db.Venues.AsNoTracking().SingleOrDefaultAsync(d => d.Id == key);
        if (venue == null)
            return NotFound();
        if (venue.Deleted != null)
            return NotFound();
        if (venue.Approved)
            return Ok(venue);
        if (!user.IsAuthenticated)
            return NotFound();
        
        var isManager = venue.Managers?.Contains(user.Id.ToString()) == true;
        if (isManager)
            return Ok(venue);
        
        if (auth.Matches(venue, user.Authorization.Edit) || 
            auth.Matches(venue, user.Authorization.EditApproval) || 
            auth.Matches(venue, user.Authorization.EditManagers) || 
            auth.Matches(venue, user.Authorization.Delete))
            return Ok(venue);
        
        return NotFound();
    }
    
    [EnableQuery]
    public async Task<ActionResult<Venue>> Post([FromBody] Venue venue)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!user.Authorization.UnlimitedVenues)
        {
            var venuesByManager = db.Venues.Where(v => v.Managers.Contains(user.Id.ToString()));
            var venuesInLastDay = await venuesByManager.CountAsync(v => v.Added >= DateTimeOffset.UtcNow.AddDays(-1));
            if (venuesInLastDay >= 3)
                return BadRequest("You have reached the limit of 3 venues created in the last 24 hours.");
            
            var totalVenues = await venuesByManager.CountAsync(v => v.Deleted == null);
            if (totalVenues >= 9)
                return BadRequest("You have reached the limit of 9 venues you are a manager of.");
        }

        if (!user.Authorization.UnlimitedSchedules)
        {
            if (venue.Schedule.Any(s => 
                    new TimeOnly(s.EndHour!.Value, s.EndMinute!.Value) - new TimeOnly(s.StartHour, s.StartMinute) > TimeSpan.FromHours(7)))
                return BadRequest("Cannot set Schedule to more than 7 hours");
            
            if (venue.ScheduleOverrides.Any(s => s.End - s.Start > TimeSpan.FromHours(7)))
                return BadRequest("Cannot set Schedule Override to more than 7 hours");
        }
        
        var hasEditApprovalAuthorization = user.IsAuthenticated && auth.Matches(venue, user.Authorization.EditApproval);
        var hasEditManagersAuthorization = user.IsAuthenticated && auth.Matches(venue, user.Authorization.EditManagers);
        
        venue.Id = IdHelper.GenerateId();
        venue.Banner = null;
        if (!hasEditApprovalAuthorization)
            venue.Approved = false;
        if (!hasEditManagersAuthorization)
            venue.Managers = [ user.Id.ToString() ];
        
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
        if (existingVenue is not { Deleted: null })
            return NotFound();

        var isManager = existingVenue.Managers?.Contains(user.Id.ToString()) == true;
        var hasEditAuthorization = user.IsAuthenticated && auth.Matches(existingVenue, user.Authorization.Edit);
        var hasEditApprovalAuthorization =
            user.IsAuthenticated && auth.Matches(existingVenue, user.Authorization.EditApproval);
        var hasEditManagersAuthorization =
            user.IsAuthenticated && auth.Matches(existingVenue, user.Authorization.EditManagers);

        if (!isManager && !hasEditAuthorization)
            venue.UpdatableProperties.RemoveAll(p =>
                p != nameof(existingVenue.Approved) && p != nameof(existingVenue.Managers));
        if (!hasEditApprovalAuthorization)
            venue.UpdatableProperties.Remove(nameof(existingVenue.Approved));
        if (!hasEditManagersAuthorization)
            venue.UpdatableProperties.Remove(nameof(existingVenue.Managers));
        
        if (!venue.GetChangedPropertyNames().Any())
            return Forbid();

        await db.LoadChangedNavigationsAsync(existingVenue, venue);
        venue.CopyChangedValues(existingVenue);
        
        if (!user.Authorization.UnlimitedSchedules)
        {
            // ReSharper disable EntityFramework.NPlusOne.Usage
            if (venue.GetChangedPropertyNames().Contains(nameof(Venue.Schedule)) &&
                existingVenue.Schedule!.Any(s =>
                    new TimeOnly(s.EndHour!.Value, s.EndMinute!.Value) - new TimeOnly(s.StartHour, s.StartMinute) >
                    TimeSpan.FromHours(7)))
                return BadRequest("Cannot set Schedule to more than 7 hours");

            if (venue.GetChangedPropertyNames().Contains(nameof(Venue.ScheduleOverrides)) &&
                existingVenue.ScheduleOverrides.Any(s => s.End - s.Start > TimeSpan.FromHours(7)))
                return BadRequest("Cannot set Schedule Override to more than 7 hours");
            // ReSharper enable EntityFramework.NPlusOne.Usage
        }

        await db.SaveChangesAsync();

        await bus.PublishAsync(new VenueUpdatedEvent(existingVenue.Id, user.Id), new DeliveryOptions { ScheduleDelay = TimeSpan.FromSeconds(5) });
        return Ok(existingVenue);
    }

    public async Task<ActionResult<Venue>> Delete([FromRoute] string key)
    {
        var venue = await db.Venues.FindAsync(key);
        if (venue is not { Deleted: null })
            return NotFound();

        var hasDeleteAuthorization = auth.Matches(venue, user.Authorization.Delete);
        var isManager = venue.Managers?.Contains(user.Id.ToString()) == true;
        if (!isManager && !hasDeleteAuthorization)
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

        var venue = await db.Venues.AsNoTracking().SingleOrDefaultAsync(d => d.Id == key);
        if (venue == null)
            return NotFound();
        if (venue.Deleted != null)
            return NotFound();

        ActionResult file;
        if (string.IsNullOrEmpty(venue.Banner))
            file = NoContent();
        else
        {
            var (stream, contentType) = await media.Download(venue.Id, venue.Banner, HttpContext.RequestAborted);
            file = File(stream, contentType);
        }
        
        if (venue.Approved)
            return file;
        if (!user.IsAuthenticated)
            return NotFound();
        
        var isManager = venue.Managers?.Contains(user.Id.ToString()) == true;
        if (isManager)
            return file;
        
        if (auth.Matches(venue, user.Authorization.Edit) || 
            auth.Matches(venue, user.Authorization.EditApproval) || 
            auth.Matches(venue, user.Authorization.EditManagers) || 
            auth.Matches(venue, user.Authorization.Delete))
            return file;

        return NotFound();
    }

    [HttpPut("odata/Venues({key})/banner")]
    public async Task<ActionResult> PutToBanner([FromRoute] string key)
    {
        var venue = await db.Venues.FindAsync(key);
        if (venue is not { Deleted: null })
            return NotFound();

        var isManager = venue.Managers?.Contains(user.Id.ToString()) == true;
        var hasEditAuthorization = user.IsAuthenticated && auth.Matches(venue, user.Authorization.Edit);
        
        if (!isManager && !hasEditAuthorization)
            return Forbid();

        if (Request.ContentLength is not > 0)
            return StatusCode(StatusCodes.Status411LengthRequired);
        if (Request.ContentLength > MAX_BANNER_BYTES)
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
        if (venue is not { Deleted: null })
            return NotFound();

        var isManager = venue.Managers?.Contains(user.Id.ToString()) == true;
        var hasEditAuthorization = user.IsAuthenticated && auth.Matches(venue, user.Authorization.Edit);
        
        if (!isManager && !hasEditAuthorization)
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
