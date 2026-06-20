using System.Text.Json.Serialization;
using FFXIVVenues.DomainData.Entities.Flags;

namespace FFXIVVenues.FlagService.Client.Events
{
    public record VenueFlaggedEvent(string VenueId, FlagCategory Category, string? Description, string? From);
}

namespace FFXIVVenues.DomainData.Entities.Flags
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FlagCategory {
        VenueEmpty,
        IncorrectInformation,
        InappropriateContent
    }
}
