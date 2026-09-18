using AutoMapper;
using FFXIVVenues.DomainData.Context;
using FFXIVVenues.DomainData.Mapping;
using FFXIVVenues.VenueService.Client.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace FFXIVVenues.ScheduleService.EventsHandlers;

public class VenueCreatedHandler(DomainDataContext db, ScheduleExpander expander)
{
    public Task Handle(VenueCreatedEvent @event)
    {
        Log.Information("Received venue created event: {event}", @event);

        var venue = db.Venues.Find(@event.VenueId);
        if (venue is null)
        {
            Log.Information("Did not update openings for venue created event: {event}; venue with the given id did not exist", @event);
            return Task.CompletedTask;
        }

        return expander.ExpandVenueToOpeningsAsync(venue);
    }

}
