using System.Net;
using FFXIVVenues.DomainData.Entities.Flags;
using FFXIVVenues.FlagService.Client.Commands;
using Wolverine;
using Wolverine.Runtime;

namespace FFXIVVenues.FlagService.Client;

public class FlagServiceClient(IWolverineRuntime runtime)
{
    public async Task SendFlagAsync(string venueId, FlagCategory category, string? description, IPAddress? ipaddress)
    {
        var flagCommand = new FlagVenueCommand(venueId, category, description, ipaddress?.ToString());
        await new MessageBus(runtime).PublishAsync(flagCommand);
    }

    public async Task DismissFlagAsync(string flagId, ulong dismissedBy)
    {
        var flagCommand = new DismissFlagCommand(flagId, dismissedBy);
        await new MessageBus(runtime).PublishAsync(flagCommand);
    }

    public async Task ResolveFlagAsync(string flagId, ulong resolvedBy)
    {
        var flagCommand = new ResolveFlagCommand(flagId, resolvedBy);
        await new MessageBus(runtime).PublishAsync(flagCommand);
    }
}