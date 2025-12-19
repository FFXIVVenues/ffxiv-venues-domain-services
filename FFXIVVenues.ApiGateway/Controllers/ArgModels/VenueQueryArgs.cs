using System.Collections.Generic;
using System.Linq;
using Domain = FFXIVVenues.DomainData.Entities.Venues;
using Dto = FFXIVVenues.VenueModels;

namespace FFXIVVenues.ApiGateway.Controllers.ArgModels;

public class VenueQueryArgs
{
    public string Search { get;set; }
    public string Manager { get;set; }
    public string DataCenter { get;set; }
    public string World { get;set; }
    public string Tags { get;set; }
    public bool? HasBanner { get;set; }
    public bool? Approved { get;set; }
    public bool? Open { get;set; }
    public bool? WithinWeek { get;set; }

    public IQueryable<Domain.Venue> ApplyDomainQueryArgs(IQueryable<Domain.Venue> query)
    {
        if (this.Search != null)
            query = query.Where(v => v.Name.ToLower().Contains(this.Search.ToLower()));
        if (this.Manager != null)
            query = query.Where(v => v.Managers.Contains(this.Manager));
        if (this.DataCenter != null)
            query = query.Where(v => v.Location.DataCenter.ToLower() == this.DataCenter.ToLower());
        if (this.World != null)
            query = query.Where(v => v.Location.World.ToLower() == this.World.ToLower());
        if (this.Tags != null)
        {
            var splitTags = this.Tags.Split(',');
            query = query.Where(v => splitTags.All(tag => v.Tags.Contains(tag)));
        }
        if (this.Approved != null)
            query = query.Where(v => v.Approved == this.Approved);
        if (this.HasBanner != null)
            query = query.Where(v => this.HasBanner.Value == (v.Banner != null));
        return query;
    }

    public IEnumerable<Dto.Venue> ApplyDtoQueryArgs(IEnumerable<Dto.Venue> query)
    {
        if (this.Open != null)
            query = query.AsEnumerable().Where(v => (v.Resolution?.IsNow ?? false) == this.Open);
        if (this.WithinWeek != null)
            query = query.AsEnumerable().Where(v => (v.Resolution?.IsWithinWeek ?? false) == this.WithinWeek);
        return query;
    }

}