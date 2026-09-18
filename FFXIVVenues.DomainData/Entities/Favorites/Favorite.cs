using FFXIVVenues.DomainData.Entities.Venues;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FFXIVVenues.DomainData.Entities.Favorites;

[Table("Favorite", Schema = "Patronage")]
[PrimaryKey(nameof(UserId), nameof(VenueId))]
[Index(nameof(UserId))]
[Index(nameof(VenueId))]
public class Favorite
{
    public ulong UserId { get; set; }
    public string VenueId { get; set; } = null!;
    public virtual Venue Venue { get; set; } = null!;
}