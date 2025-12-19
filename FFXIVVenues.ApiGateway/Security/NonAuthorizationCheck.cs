using System.Collections.Generic;
using System.Linq;
using FFXIVVenues.DomainData.Entities.Venues;

namespace FFXIVVenues.ApiGateway.Security;

public class NonAuthorizationCheck : IAuthorizationCheck
{

    public bool CanNot(Operation op, Venue _ = null) => !Can(op, _);

    public bool Can(Operation op, Venue venue = null) => 
        op == Operation.Read && venue?.Approved == true;
        
    public IQueryable<Venue> Can(Operation op, IQueryable<Venue> queryable) =>
        op == Operation.Read ? queryable.Where(i => i.Approved) : new List<Venue>().AsQueryable();

}