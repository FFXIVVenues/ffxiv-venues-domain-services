namespace FFXIVVenues.FlagService.Client.Events;

public record FlagDismissedEvent(string FlagId, string VenueId, long ResolvedBy);
