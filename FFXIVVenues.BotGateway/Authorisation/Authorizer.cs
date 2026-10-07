using System.Linq;
using FFXIVVenues.BotGateway.Authorisation.Configuration;
using DtoVenue = FFXIVVenues.VenueModels.Venue;
using DomainVenue = FFXIVVenues.DomainData.Entities.Venues.Venue;

namespace FFXIVVenues.BotGateway.Authorisation;

public class Authorizer(AuthorisationConfiguration configuration) : IAuthorizer
{
    
    public const string VENUE_SOURCE_KEY = "_VenueSource";
    public const string MASTER_SOURCE_KEY = "_Master";

    
    public AuthorizationResult Authorize(ulong user, Permission permission)
    {
        if (configuration.Master.Contains(user))
            return new(true, MASTER_SOURCE_KEY, permission, user);

        foreach (var set in configuration.PermissionSets)
        {
            if (!set.Members.Contains(user)) continue;
            if (set.Permissions.Contains(permission))
                return new(true, set.Name, permission, user);
        }

        return new(false, null, permission, user);
    }

    
    public DtoAuthorizationResult Authorize(ulong user, Permission permission, DtoVenue venue)
    {
        if (configuration.Master.Contains(user))
            return new(true, MASTER_SOURCE_KEY, permission, user, venue);

        var localPermission = venue is not null ? permission.ToLocalPermission(venue.Location?.DataCenter) : null;
        if (venue != null && venue.Managers.Contains(user.ToString()))
        {
            if (configuration.ManagerPermissions.Contains(permission))
                return new(true, VENUE_SOURCE_KEY, permission, user, venue);
            if (localPermission.HasValue && configuration.ManagerPermissions.Contains(localPermission.Value))
                return new(true, VENUE_SOURCE_KEY, localPermission.Value, user, venue);
        }
        foreach (var set in configuration.PermissionSets)
        {
            if (!set.Members.Contains(user)) continue;
            if (set.Permissions.Contains(permission))
                return new(true, set.Name, permission, user, venue);
            if (localPermission.HasValue && set.Permissions.Contains(localPermission.Value))
                return new(true, set.Name, localPermission.Value, user, venue);
        }

        return new(false, null, permission, user, venue);
    }

    public DomainAuthorizationResult Authorize(ulong user, Permission permission, DomainVenue venue)
    {
        if (configuration.Master.Contains(user))
            return new(true, MASTER_SOURCE_KEY, permission, user, venue);

        var localPermission = venue is not null ? permission.ToLocalPermission(venue?.Location?.DataCenter) : null;
        if (venue != null && venue.Managers.Contains(user.ToString()))
        {
            if (configuration.ManagerPermissions.Contains(permission))
                return new(true, VENUE_SOURCE_KEY, permission, user, venue);
            if (localPermission.HasValue && configuration.ManagerPermissions.Contains(localPermission.Value))
                return new(true, VENUE_SOURCE_KEY, localPermission.Value, user, venue);
        }
        foreach (var set in configuration.PermissionSets)
        {
            if (!set.Members.Contains(user)) continue;
            if (set.Permissions.Contains(permission))
                return new(true, set.Name, permission, user, venue);
            if (localPermission.HasValue && set.Permissions.Contains(localPermission.Value))
                return new(true, set.Name, localPermission.Value, user, venue);
        }

        return new(false, null, permission, user, venue);
    }
}