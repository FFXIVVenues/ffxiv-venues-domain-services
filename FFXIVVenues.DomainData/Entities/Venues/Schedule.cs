using System;
using System.ComponentModel.DataAnnotations.Schema;
using FFXIVVenues.VenueModels;
using Microsoft.EntityFrameworkCore;

namespace FFXIVVenues.DomainData.Entities.Venues;

[Table("Schedules", Schema = nameof(Entities.Venues))]
// Commencing may be a factor due to offset of biweekly
// but can't be added without making it not-nullable
// ID at some point
[PrimaryKey(nameof(VenueId), nameof(Day), nameof(StartHour), nameof(StartMinute), nameof(IntervalType), nameof(IntervalArgument))]
public class Schedule
{
    [ForeignKey(nameof(Venue))] protected string VenueId { get; set; }
    public IntervalType IntervalType { get; set; } = IntervalType.EveryXWeeks;
    public int IntervalArgument { get; set; } = 1;
    public DateTimeOffset? Commencing { get; set; }
    public Day Day { get; set; }
    public ushort StartHour { get; set; }
    public ushort StartMinute { get; set; }
    public ushort? EndHour { get; set; }
    public ushort? EndMinute { get; set; }
    public string TimeZone { get; set; }
    public virtual Venue? Venue { get; set; }
}