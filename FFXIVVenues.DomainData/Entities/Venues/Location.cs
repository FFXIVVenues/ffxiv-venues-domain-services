using System.ComponentModel.DataAnnotations.Schema;
using FFXIVVenues.DomainData.Helpers;
using Microsoft.EntityFrameworkCore;

namespace FFXIVVenues.DomainData.Entities.Venues;

[Table("Locations", Schema = nameof(Entities.Venues))]
[PrimaryKey(nameof(Id))]
[Index(nameof(DataCenter), 
    nameof(World), 
    nameof(District), 
    nameof(Ward), 
    nameof(Plot),
    nameof(Apartment),
    nameof(Room),
    nameof(Subdivision),    
    Name = "DCAddress")]
[Index(nameof(World), 
    nameof(District), 
    nameof(Ward), 
    nameof(Plot),
    nameof(Apartment),
    nameof(Room),
    nameof(Subdivision),
    Name = "WorldAddress")]
[Index(nameof(Override), Name ="Override")] 
public class Location
{
    public string Id { get; set; } = IdHelper.GenerateId(8);
    public string DataCenter { get; set; }
    public string World { get; set; }
    public string District { get; set; }
    public ushort Ward { get; set; }
    public ushort Plot { get; set; }
    public ushort Apartment { get; set; }
    public ushort Room { get; set; }
    public bool Subdivision { get; set; }
    public string? Override { get; set; }
    
    public virtual List<Venue> Venues { get; set; }
}