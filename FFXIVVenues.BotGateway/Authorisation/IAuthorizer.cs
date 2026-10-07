using DtoVenue = FFXIVVenues.VenueModels.Venue;
using DomainVenue = FFXIVVenues.DomainData.Entities.Venues.Venue;

namespace FFXIVVenues.BotGateway.Authorisation;

public interface IAuthorizer
{
    AuthorizationResult Authorize(ulong user, Permission permission);
    DtoAuthorizationResult Authorize(ulong user, Permission permission, DtoVenue venue);
    DomainAuthorizationResult Authorize(ulong user, Permission permission, DomainVenue venue);
}