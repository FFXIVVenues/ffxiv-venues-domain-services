using Discord;
using Discord.WebSocket;

namespace FFXIVVenues.Veni.Infrastructure.Commands
{
    public interface ICommandFactory
    {
        SlashCommandProperties GetSlashCommand();
    }

}
