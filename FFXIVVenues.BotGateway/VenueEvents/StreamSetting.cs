using FFXIVVenues.BotGateway.Infrastructure.Persistence.Abstraction;

namespace FFXIVVenues.BotGateway.VenueEvents;

public record EventStreamChannel(ulong ChannelId, StreamableEvent EventType) : IEntity
{
    public string id => $"{ChannelId}_{EventType}";
}
