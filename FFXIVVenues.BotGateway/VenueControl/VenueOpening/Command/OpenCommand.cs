using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FFXIVVenues.BotGateway.Infrastructure.Commands;
using FFXIVVenues.BotGateway.Infrastructure.Commands.Attributes;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Infrastructure.Intent;

namespace FFXIVVenues.BotGateway.VenueControl.VenueOpening.Command;

[DiscordCommand("open", "Open your venue! It will show under 'open now' on the website.")]
internal class Handler(IIntentHandlerProvider intentProvider) : ICommandHandler
{
    public Task HandleAsync(SlashCommandVeniInteractionContext slashCommand) =>
        intentProvider.HandleIntent(IntentNames.Operation.Open, slashCommand);
}