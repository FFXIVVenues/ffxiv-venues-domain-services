using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;

namespace FFXIVVenues.Veni.VenueEvents;

public record EventStreamChannel(ulong ChannelId, StreamableEvent EventType) : IEntity
{
    public string id => $"{ChannelId}_{EventType}";
}
