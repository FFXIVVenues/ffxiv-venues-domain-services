using System;

namespace FFXIVVenues.VenueService.Client.Events;

public record VenueCreatedEvent(string VenueId, ulong Actor) 
{
    public DateTimeOffset At { get; set; } = DateTimeOffset.UtcNow;
}
