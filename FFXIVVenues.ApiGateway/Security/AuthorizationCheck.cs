using System;
using System.Collections.Generic;
using System.Linq;
using FFXIVVenues.DomainData.Entities.Venues;

namespace FFXIVVenues.ApiGateway.Security;

public class AuthorizationCheck(AuthorizationKey key) : IAuthorizationCheck
{
    public bool CanNot(Operation op, Venue venue = null) => !Can(op, venue);

    public bool Can(Operation op, Venue venue = null)
    {
        if (op == Operation.Create && venue != null)
            throw new InvalidOperationException("Cannot authorise Create permission against an existing item.");
            
        if (op == Operation.Create)
            return key.Create;
            
        if (venue == null)
            return false;
            
        return op switch
        {
            Operation.Read => venue.Approved || key.ReadUnapproved,
            Operation.Approve => key.Approve,
            Operation.Create => key.Create,
            Operation.Update => key.Update,
            Operation.Delete => key.Delete,
            _ => false
        };
    }

    public IQueryable<Venue> Can(Operation op, IQueryable<Venue> queryable)
    {
        if (op == Operation.Create)
            throw new InvalidOperationException("Cannot query venues on Create permission.");
            
        var opAuthorised = op switch
        {
            Operation.Read => true,
            Operation.Approve => key.Approve,
            Operation.Create => key.Create,
            Operation.Update => key.Update,
            Operation.Delete => key.Delete,
            _ => false
        };

        if (!opAuthorised)
            return new List<Venue>().AsQueryable();

        if (!key.ReadUnapproved)
            return queryable.Where(i => i.Approved);

        return queryable;
    }
        
        
}