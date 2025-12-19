using System.Linq;
using FFXIVVenues.DomainData.Entities.Venues;

namespace FFXIVVenues.ApiGateway.Security;

public interface IAuthorizationCheck
{
    bool CanNot(Operation op, Venue venue = null);

    bool Can(Operation op, Venue venue = null);

    IQueryable<Venue> Can(Operation op, IQueryable<Venue> queryable);
}