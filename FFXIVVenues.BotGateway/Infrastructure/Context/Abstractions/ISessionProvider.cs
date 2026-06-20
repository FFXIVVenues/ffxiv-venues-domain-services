using Discord.WebSocket;
using FFXIVVenues.BotGateway.Infrastructure.Context.SessionHandling;

namespace FFXIVVenues.BotGateway.Infrastructure.Context.Abstractions
{
    public interface ISessionProvider
    {
        Session GetSession(SocketMessage message);

        Session GetSession(SocketMessageComponent message);

        Session GetSession(SocketSlashCommand message);
            
        Session GetSession(string key);
    }
}