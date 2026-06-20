using FFXIVVenues.VenueModels;

namespace FFXIVVenues.BotGateway.Authorisation;

public interface IAuthorizer
{
    AuthorizationResult Authorize(ulong user, Permission permission, Venue venue = null);
}