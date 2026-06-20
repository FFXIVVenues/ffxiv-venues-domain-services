using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.VenueCreation.Command
{
    public static class CreateCommand
    {

        public const string COMMAND_NAME = "create";

        internal class Factory : ICommandFactory
        {

            public SlashCommandProperties GetSlashCommand()
            {
                return new SlashCommandBuilder()
                    .WithName(COMMAND_NAME)
                    .WithDescription("Create a new venue! 🥰")
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
                this._intentProvider.HandleIntent(IntentNames.Operation.Create, slashCommand);

        }

    }
}
