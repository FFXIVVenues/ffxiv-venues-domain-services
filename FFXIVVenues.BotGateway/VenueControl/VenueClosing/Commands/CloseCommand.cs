using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;

namespace FFXIVVenues.Veni.VenueControl.VenueClosing.Commands
{
    public static class CloseCommand
    {

        public const string COMMAND_NAME = "close";
        internal class Factory : ICommandFactory
        {

            public SlashCommandProperties GetSlashCommand()
            {
                return new SlashCommandBuilder()
                    .WithName(COMMAND_NAME)
                    .WithDescription("If open, close the venue early, else, keep the venue closed for the next 18 hours.")
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
                this._intentProvider.HandleIntent(IntentNames.Operation.Close, slashCommand);
        }

    }
}
