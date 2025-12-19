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
            
        if (op == Operation.Read && venue.Approved)
            return true;
            
        if (key.Scope == "all" || (key.Scope == "approved" && venue.Approved))
            return op switch
            {
                Operation.Read => true,
                Operation.Approve => key.Approve,
                Operation.Create => key.Create,
                Operation.Update => key.Update,
                Operation.Delete => key.Delete,
                _ => false
            };

        return venue.ScopeKey == key.Key && op switch
        {
            Operation.Read => true,
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
            throw new InvalidOperationException("Cannot items venues on Create permission.");
            
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

        if (key.Scope == "all")
            return queryable;
            
        if (op == Operation.Read || key.Scope == "approved")
            return queryable.Where(i => i.Approved || i.ScopeKey == key.Key);
            
        return queryable.Where(i => i.ScopeKey == key.Key);
    }
        
        
}