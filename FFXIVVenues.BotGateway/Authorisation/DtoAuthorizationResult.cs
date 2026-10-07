using DtoVenue = FFXIVVenues.VenueModels.Venue;
using DomainVenue = FFXIVVenues.DomainData.Entities.Venues.Venue;

namespace FFXIVVenues.BotGateway.Authorisation;

public record AuthorizationResult(bool Authorized, string Source, Permission Permission, ulong UserId);
public record DtoAuthorizationResult(bool Authorized, string Source, Permission Permission, ulong UserId, DtoVenue Venue) 
    : AuthorizationResult(Authorized, Source, Permission, UserId);
public record DomainAuthorizationResult(bool Authorized, string Source, Permission Permission, ulong UserId, DomainVenue Venue)
    : AuthorizationResult(Authorized, Source, Permission, UserId);
