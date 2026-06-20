using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;

namespace FFXIVVenues.Veni.UserSupport
{
    public static class HelpCommand
    {

        public const string COMMAND_NAME = "help";

        internal class CommandFactory : ICommandFactory
        {

            public SlashCommandProperties GetSlashCommand()
            {
                return new SlashCommandBuilder()
                    .WithName(COMMAND_NAME)
                    .WithDescription("Shows information on what I can do!")
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
                this._intentProvider.HandleIteruptIntent(IntentNames.Interupt.Help, slashCommand);

        }

    }
}
