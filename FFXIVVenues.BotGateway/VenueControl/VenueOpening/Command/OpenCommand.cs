using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FFXIVVenues.BotGateway.Infrastructure.Commands;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Infrastructure.Intent;

namespace FFXIVVenues.BotGateway.VenueControl.VenueOpening.Command
{
    public static class OpenCommand
    {

        public const string COMMAND_NAME = "open";

        internal class Factory : ICommandFactory 
        {

            public SlashCommandProperties GetSlashCommand()
            {
                return new SlashCommandBuilder()
                    .WithName(COMMAND_NAME)
                    .WithDescription("Open your venue! It will show under 'open now' on the website.")
                    .Build();
            }

        }

        internal class Handler : ICommandHandler
        {
            private readonly IIntentHandlerProvider _intentProvider;

            public Handler(IIntentHandlerProvider intentProvider)
            {
                this._intentProvider = intentProvider;
            }

            public Task HandleAsync(SlashCommandVeniInteractionContext slashCommand) =>
                this._intentProvider.HandleIntent(IntentNames.Operation.Open, slashCommand);

        }

    }
}
