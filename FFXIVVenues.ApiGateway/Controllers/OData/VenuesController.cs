using FFXIVVenues.DomainData.Context;
using FFXIVVenues.DomainData.Entities.Venues;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace FFXIVVenues.ApiGateway.Controllers.OData;

public class VenuesController(DomainDataContext domainData) : ODataController
{
    [EnableQuery]
    public ActionResult<IQueryable<Venue>> Get() =>
        Ok(domainData.Venues.AsNoTracking().Where(v => v.Approved));

    [EnableQuery]
    public ActionResult<Venue> Get([FromRoute] string key)
    {
        var venue = domainData.Venues.AsNoTracking().SingleOrDefault(d => d.Id == key && d.Approved);
        if (venue == null)
            return NotFound();
        return Ok(venue);
    }
}
