using FFXIVVenues.DomainData.Context;
using FFXIVVenues.VenueService.Client.Events;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace FFXIVVenues.ScheduleService.EventsHandlers;

public class VenueDeletedHandler(DomainDataContext db, ScheduleExpander expander)
{
    public Task Handle(VenueDeletedEvent @event)
    {
        Log.Information("Received venue deleted event: {event}", @event);

        var venue = db.Venues.Find(@event.VenueId);
        if (venue is null)
        {
            Log.Information("Did not update openings for venue deleted event: {event}; venue with the given id did not exist", @event);
            return Task.CompletedTask;
        }

        return expander.ExpandVenueToOpeningsAsync(venue);
    }
}
