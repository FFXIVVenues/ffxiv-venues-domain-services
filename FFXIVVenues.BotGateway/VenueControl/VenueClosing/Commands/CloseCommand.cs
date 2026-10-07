using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FFXIVVenues.BotGateway.Infrastructure.Commands;
using FFXIVVenues.BotGateway.Infrastructure.Commands.Attributes;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Infrastructure.Intent;

namespace FFXIVVenues.BotGateway.VenueControl.VenueClosing.Commands;

[DiscordCommand("close", "If open, close the venue early, else, keep the venue closed for the next 18 hours.")]
internal class CloseCommand(IIntentHandlerProvider intentProvider) : ICommandHandler
{
    public Task HandleAsync(SlashCommandVeniInteractionContext slashCommand) =>
        intentProvider.HandleIntent(IntentNames.Operation.Close, slashCommand);
}
