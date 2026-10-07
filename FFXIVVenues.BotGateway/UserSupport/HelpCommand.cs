using System.Threading.Tasks;
using FFXIVVenues.BotGateway.Infrastructure.Commands;
using FFXIVVenues.BotGateway.Infrastructure.Commands.Attributes;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Infrastructure.Intent;

namespace FFXIVVenues.BotGateway.UserSupport;

[DiscordCommand("help", "Shows information on what I can do!")]
public class HelpCommand(IIntentHandlerProvider intentProvider) : ICommandHandler
{
    public Task HandleAsync(SlashCommandVeniInteractionContext slashCommand) =>
        intentProvider.HandleIteruptIntent(IntentNames.Interupt.Help, slashCommand);
}

