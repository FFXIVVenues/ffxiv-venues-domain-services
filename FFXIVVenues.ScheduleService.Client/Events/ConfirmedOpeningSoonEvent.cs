using System;
using System.Collections.Generic;
using System.Text;

namespace FFXIVVenues.ScheduleService.Client.Events;

public record ConfirmedOpeningSoonEvent(string VenueId, DateTimeOffset From, DateTimeOffset To);
