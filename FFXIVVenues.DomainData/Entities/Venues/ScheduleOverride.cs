using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FFXIVVenues.DomainData.Entities.Venues;

[Table("ScheduleOverrides", Schema = nameof(Venues))]
[PrimaryKey(nameof(VenueId), nameof(Start))]
public class ScheduleOverride
{
    [ForeignKey(nameof(Venue))] protected string VenueId { get; set; }
    public bool Open { get; set; }
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset End { get; set; }
    
    public virtual Venue Venue { get; set; }
}