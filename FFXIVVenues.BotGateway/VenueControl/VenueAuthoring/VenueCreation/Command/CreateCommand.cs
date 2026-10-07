using System.Threading.Tasks;
using FFXIVVenues.BotGateway.Infrastructure.Commands;
using FFXIVVenues.BotGateway.Infrastructure.Commands.Attributes;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Infrastructure.Intent;

namespace FFXIVVenues.BotGateway.VenueControl.VenueAuthoring.VenueCreation.Command;

[DiscordCommand("create", "Create a new venue! 🥰")]
internal class CreateCommand(IIntentHandlerProvider intentProvider) : ICommandHandler
{
    public Task HandleAsync(SlashCommandVeniInteractionContext slashCommand) =>
        intentProvider.HandleIntent(IntentNames.Operation.Create, slashCommand);

}
