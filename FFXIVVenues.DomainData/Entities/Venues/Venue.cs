using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FFXIVVenues.DomainData.Entities.Favorites;
using Microsoft.EntityFrameworkCore;

namespace FFXIVVenues.DomainData.Entities.Venues;

[Table("Venues", Schema = nameof(Entities.Venues))]
public class Venue
{
    [Key] public string Id { get; init; }
    public string Name { get; set; }
    public string? Banner { get; set; }
    public DateTimeOffset Added { get; set; }
    public DateTimeOffset? LastModified { get; set; }
    public virtual List<string>? Description { get; set; } = new ();
    public virtual Location Location { get; set; } = new ();
    public Uri? Website { get; set; }
    public Uri? Discord { get; set; }
    public bool Hiring { get; set; }
    public bool Sfw { get; set; }
    [DeleteBehavior(DeleteBehavior.Cascade)] public virtual List<Schedule> Schedule { get; set; } = new ();
    [DeleteBehavior(DeleteBehavior.Cascade)] public virtual List<ScheduleOverride> ScheduleOverrides { get; set; } = new ();
    [DeleteBehavior(DeleteBehavior.Cascade)] public virtual List<Notice> Notices { get; set; } = new ();
    public virtual List<string>? Managers { get; set; } = new ();
    public virtual List<string>? Tags { get; set; } = new ();
    public string? MareCode { get; set; }
    public string? MarePassword { get; set; }
    public DateTimeOffset? Deleted { get; set; }
    public bool Approved { get; set; }
    public string? ScopeKey { get; set; }
    [DeleteBehavior(DeleteBehavior.Cascade)] public virtual List<Favorite> Favorites { get; set; } = new();


    public Venue()
    {
        this.Added = DateTimeOffset.UtcNow;
    }
    
}