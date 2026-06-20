using System.Net;
using FFXIVVenues.DomainData.Entities.Flags;
using FFXIVVenues.FlagService.Client.Commands;
using Wolverine;

namespace FFXIVVenues.FlagService.Client;

public interface IFlagServiceClient
{
    Task SendFlagAsync(string venueId, FlagCategory category, string? description, IPAddress? ipaddress);
    Task DismissFlagAsync(string flagId, long dismissedBy);
    Task ResolveFlagAsync(string flagId, long resolvedBy);
}

public class FlagServiceClient (IMessageBus bus) : IFlagServiceClient
{
    public async Task SendFlagAsync(string venueId, FlagCategory category, string? description, IPAddress? ipaddress)
    {
        var flagCommand = new FlagVenueCommand(venueId, category, description, ipaddress?.ToString());
        await bus.PublishAsync(flagCommand);
    }

    public async Task DismissFlagAsync(string flagId, long dismissedBy)
    {
        var flagCommand = new DismissFlagCommand(flagId, dismissedBy);
        await bus.PublishAsync(flagCommand);
    }

    public async Task ResolveFlagAsync(string flagId, long resolvedBy)
    {
        var flagCommand = new ResolveFlagCommand(flagId, resolvedBy);
        await bus.PublishAsync(flagCommand);
    }
}