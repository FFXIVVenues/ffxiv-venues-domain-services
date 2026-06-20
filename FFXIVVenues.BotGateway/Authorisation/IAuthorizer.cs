using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.Authorisation;

public interface IAuthorizer
{
    AuthorizationResult Authorize(ulong user, Permission permission, Venue venue = null);
}