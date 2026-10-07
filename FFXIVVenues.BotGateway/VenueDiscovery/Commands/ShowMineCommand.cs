using System.Threading.Tasks;
using FFXIVVenues.BotGateway.Infrastructure.Commands;
using FFXIVVenues.BotGateway.Infrastructure.Commands.Attributes;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Infrastructure.Intent;

namespace FFXIVVenues.BotGateway.VenueDiscovery.Commands;

[DiscordCommand("showmine", "Shows your venue(s)!")]
internal class ShowMineCommand(IIntentHandlerProvider intentProvider) : ICommandHandler
{
    public Task HandleAsync(SlashCommandVeniInteractionContext slashCommand) =>
        intentProvider.HandleIntent(IntentNames.Operation.Show, slashCommand);
}