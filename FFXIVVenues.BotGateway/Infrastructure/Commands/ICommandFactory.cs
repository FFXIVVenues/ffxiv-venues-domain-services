using Discord;
using Discord.WebSocket;

namespace FFXIVVenues.BotGateway.Infrastructure.Commands
{
    public interface ICommandFactory
    {
        SlashCommandProperties GetSlashCommand();
    }

}
