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
        Ok(db.Venues.AsNoTracking().Where(v => v.Approved && v.Deleted == null));

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

        var existingVenue = await db.Venues.FindAsync(key);
        if (existingVenue == null || existingVenue.Deleted != null)
            return NotFound();
        
        if (existingVenue.Managers?.Contains(user.Id.ToString()) != null)
            return Forbid();

        venue.CopyChangedValues(existingVenue);
        await db.SaveChangesAsync();

        changeBroker.Queue(ObservableOperation.Update, existingVenue);
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

        return venue;
    }

}
