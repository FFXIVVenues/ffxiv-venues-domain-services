using FFXIVVenues.DomainData.Entities.Flags;

namespace FFXIVVenues.FlagService.Client.Events;

public record VenueFlaggedEvent(string VenueId, FlagCategory Category, string Description);
