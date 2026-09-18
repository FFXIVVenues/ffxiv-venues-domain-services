using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FFXIVVenues.DomainData.Entities.Venues;

[Table("Opening", Schema = nameof(Entities.Venues))]
[Index(nameof(VenueId))]
[Index(nameof(Start))]
public class Opening : IEquatable<Opening>
{
    [Key][DatabaseGenerated(DatabaseGeneratedOption.Identity)]  
    public string Id { get; init; }
    public string VenueId { get; set; }
    public virtual Venue Venue { get; set; } = null!;
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset End { get; set; }
    public OpeningSource Source { get; set; }
    public OpeningStatus Status { get; set; }
    public BroadcastStatus Broadcasted { get; set; }

    public bool Equals(Opening? other)
    {
        return other is not null &&
            this.VenueId == other.VenueId &&
            this.Start == other.Start &&
            this.End == other.End &&
            this.Source == other.Source &&
            this.Status == other.Status;
    }

    public override int GetHashCode() =>
        HashCode.Combine(this.VenueId, this.Start, this.End, this.Source);

}

public enum BroadcastStatus
{
    NotBroadcasted,
    BroadcastedUnconfirmed,
    BroadcastedConfirmed,
}

public enum OpeningStatus
{
    OpeningUnconfirmed,
    OpeningConfirmed,
    OpeningCancelled
}

public enum OpeningSource
{
    Schedule, 
    Adhoc // from schedule overrides
}