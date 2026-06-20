using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;

namespace FFXIVVenues.Veni.VenueDiscovery.Commands;

public static class ShowOpenCommand
{

    public const string COMMAND_NAME = "showopen";
    public const string OPTION_WORLD = "world";
    public const string OPTION_DATACENTER = "datacenter";

    private static string[] _worlds = new[]
    {
        "Adamantoise",
        "Cactuar",
        "Faerie",
        "Gilgamesh",
        "Jenova",
        "Midgardsormr",
        "Sargatanas",
        "Siren",

        "Behemoth",
        "Excalibur",
        "Exodus",
        "Famfrit",
        "Hyperion",
        "Lamia",
        "Leviathan",
        "Ultros",

        "Balmung",
        "Brynhildr",
        "Coeurl",
        "Diabolos",
        "Goblin",
        "Malboro",
        "Mateus",
        "Zalera"
    };

    private static string[] _dataCenters = new[]
    {
        "Aether",
        "Crystal",
        "Primal"
    };

    internal class CommandFactory : ICommandFactory
    {

        public SlashCommandProperties GetSlashCommand()
        {
            //var worldOption = new SlashCommandOptionBuilder()
            //   .WithName(OPTION_WORLD)
            //   .WithType(ApplicationCommandOptionType.Integer)
            //   .WithDescription("The world to filter to");
            //for (var i = 0; i < _worlds.Length; i++)
            //    worldOption.AddChoice(_worlds[i], i);

            //var dataCenterOption = new SlashCommandOptionBuilder()
            //   .WithName(OPTION_DATACENTER)
            //   .WithType(ApplicationCommandOptionType.Integer)
            //   .WithDescription("The data center to filter to");
            //for (var i = 0; i < _dataCenters.Length; i++)
            //    dataCenterOption.AddChoice(_dataCenters[i], i);

            return new SlashCommandBuilder()
                .WithName(COMMAND_NAME)
                .WithDescription("Show venues that are open right now!")
                //.AddOption(worldOption)
                //.AddOption(dataCenterOption)
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
            this._intentProvider.HandleIntent(IntentNames.Operation.ShowOpen, slashCommand);

    }

}