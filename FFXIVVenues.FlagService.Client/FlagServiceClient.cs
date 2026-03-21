using System.Net;
using FFXIVVenues.DomainData.Entities.Flags;
using FFXIVVenues.FlagService.Client.Commands;
using Wolverine;

namespace FFXIVVenues.FlagService.Client;

public interface IFlagServiceClient
{
    Task SendFlagAsync(string venueId, FlagCategory category, string? description, IPAddress? ipaddress);
}

public class FlagServiceClient (IMessageBus bus) : IFlagServiceClient
{
    public async Task SendFlagAsync(string venueId, FlagCategory category, string? description, IPAddress? ipaddress)
    {
        var flagCommand = new FlagVenueCommand(venueId, category, description, ipaddress?.ToString());
        await bus.PublishAsync(flagCommand);
    }
}