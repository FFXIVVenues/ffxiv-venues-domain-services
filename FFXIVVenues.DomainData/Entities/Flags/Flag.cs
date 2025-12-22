using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace FFXIVVenues.DomainData.Entities.Flags;

[Table("Flag", Schema = "VenueFlags")]
[Index(nameof (VenueId))]
public class Flag
{
    [Key] [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; set; }
    public string VenueId { get; set;  }
    public FlagCategory Category { get; set; }
    public string Description { get; set; }
    public DateTime Timestamp { get; set;  } = DateTime.UtcNow;
    public string SourceAddress { get; set;  }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FlagCategory {
    VenueEmpty,
    IncorrectInformation,
    InappropriateContent
}