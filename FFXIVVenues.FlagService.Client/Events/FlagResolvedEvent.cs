namespace FFXIVVenues.FlagService.Client.Events;

public record FlagResolvedEvent(string FlagId, string VenueId, long ResolvedBy);
