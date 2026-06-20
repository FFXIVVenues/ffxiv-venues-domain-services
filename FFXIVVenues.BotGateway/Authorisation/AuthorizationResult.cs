using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.Authorisation;

public record AuthorizationResult(bool Authorized, string Source, Permission Permission, ulong UserId, Venue Venue);