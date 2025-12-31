using FFXIVVenues.DomainData.Entities.Flags;

namespace FFXIVVenues.FlagService.Client.Commands;

public record FlagVenueCommand(string VenueId, FlagCategory Category, string Description, string? SourceAddress);
