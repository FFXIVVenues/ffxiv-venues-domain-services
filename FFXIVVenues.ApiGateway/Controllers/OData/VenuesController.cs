using FFXIVVenues.ApiGateway.Helpers;
using FFXIVVenues.ApiGateway.Observability;
using FFXIVVenues.ApiGateway.Security;
using FFXIVVenues.DomainData.Context;
using FFXIVVenues.DomainData.Entities.Venues;
using FFXIVVenues.VenueModels.Observability;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace FFXIVVenues.ApiGateway.Controllers.OData;

public class VenuesController(DomainDataContext db, IChangeBroker changeBroker, ICurrentUser user) : ODataController
{

    [EnableQuery]
    public ActionResult<IQueryable<Venue>> Get() =>
        Ok(db.Venues.AsNoTracking().Where(v => v.Approved));

    [EnableQuery]
    public async Task<ActionResult<Venue>> Get([FromRoute] string key)
    {
        var venue = await db.Venues.AsNoTracking().SingleOrDefaultAsync(d => d.Id == key && d.Approved);
        if (venue == null)
            return NotFound();
        return Ok(venue);
    }

    public async Task<ActionResult<Venue>> Post([FromBody] Venue venue)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var venuesCreatedInLast24Hours = await db.Venues.CountAsync(
            v => v.Added >= DateTimeOffset.UtcNow.AddDays(-1) 
              && v.Managers.Contains(user.Id.ToString()));
        if (venuesCreatedInLast24Hours >= 3)
            return Forbid("You have reached the limit of 3 venues created in the last 24 hours.");

        // What if someone doesn't own but manages many venues...
        var venuesOwnedByManager = await db.Venues.CountAsync(
            v => v.Deleted == null 
              && v.Managers.Contains(user.Id.ToString()));
        if (venuesOwnedByManager >= 6)  
            return Forbid("You have reached the limit of 6 venues owned.");

        venue.Id = IdHelper.GenerateId();
        venue.Banner = null;
        venue.Approved = false;
        venue.Managers = [ user.Id.ToString() ];
        await db.Venues.AddAsync(venue);
        await db.SaveChangesAsync();

        changeBroker.Queue(ObservableOperation.Create, venue);
        return Created(venue);
    }

    public async Task<ActionResult<Venue>> Patch([FromRoute] string key, [FromBody] Delta<Venue> venue)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existingVenue = await db.Venues.SingleOrDefaultAsync(d => d.Id == key);
        if (existingVenue == null)
            return NotFound();

        if (existingVenue.Managers == null || !existingVenue.Managers.Contains(user.Id.ToString()))
            return Forbid();

        // Do we need this anymore??
        var allowedChangableProps = new List<string>
        {
            nameof(Venue.Name),
            nameof(Venue.Description),
            nameof(Venue.Location),
            nameof(Venue.Website),
            nameof(Venue.Discord),
            nameof(Venue.Sfw),
            nameof(Venue.Schedule),
            nameof(Venue.ScheduleOverrides),
            nameof(Venue.Notices),
            nameof(Venue.Managers),
            nameof(Venue.Tags),
        };

        var changedProps = venue.GetChangedPropertyNames();
        var disallowedProps = changedProps.Except(allowedChangableProps).ToList();
        if (disallowedProps.Any())
            return Forbid($"The following fields could not be changed: {string.Join(", ", disallowedProps)}.");

        venue.CopyChangedValues(existingVenue);
        await db.SaveChangesAsync();

        changeBroker.Queue(ObservableOperation.Update, existingVenue);
        return Ok(existingVenue);
    }

}
