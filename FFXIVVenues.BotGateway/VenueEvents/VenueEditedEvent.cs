using FFXIVVenues.DomainData.Entities.Flags;

namespace FFXIVVenues.BotGateway.VenueEvents;

public record VenueCreatedEvent(string VenueId, ulong UserId);
public record VenueEditEvent(string VenueId, ulong UserId);
public record VenueApprovedEvent(string VenueId, ulong UserId);
public record VenueDeletedEvent(string VenueId, string VenueName, ulong UserId);
