using System.Threading.Tasks;
using Discord;
using FFXIVVenues.BotGateway.Infrastructure.Commands;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Infrastructure.Intent;

namespace FFXIVVenues.BotGateway.VenueDiscovery.Commands
{
    public static class ShowMineCommand
    {

        public const string COMMAND_NAME = "showmine";

        internal class CommandFactory : ICommandFactory
        {

            public SlashCommandProperties GetSlashCommand()
            {
                return new SlashCommandBuilder()
                    .WithName(COMMAND_NAME)
                    .WithDescription("Shows your venue(s)!")
                    .Build();
            }

        }

        internal class CommandHandler : ICommandHandler
        {
            private readonly IIntentHandlerProvider _intentProvider;

            public CommandHandler(IIntentHandlerProvider intentProvider)
            {
                this._intentProvider = intentProvider;
            }

            public Task HandleAsync(SlashCommandVeniInteractionContext slashCommand) =>
                this._intentProvider.HandleIntent(IntentNames.Operation.Show, slashCommand);

        }

    }
}
