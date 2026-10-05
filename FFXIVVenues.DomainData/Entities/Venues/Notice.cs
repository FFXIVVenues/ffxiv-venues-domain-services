using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FFXIVVenues.DomainData.Helpers;
using FFXIVVenues.VenueModels;
using Microsoft.EntityFrameworkCore;

namespace FFXIVVenues.DomainData.Entities.Venues;

[Table("Notices", Schema = nameof(Venues))]
[PrimaryKey(nameof(VenueId), nameof(Id))]
public class Notice
{
    [ForeignKey(nameof(Venue))] protected string VenueId { get; set; }
    public string Id { get; init; } = IdHelper.GenerateId(3);
    public DateTimeOffset? Start { get; set; }
    public DateTimeOffset? End { get; set; }
    public NoticeType Type { get; set; }
    public string Message { get; set; }
    
    public virtual Venue Venue { get; set; }
}