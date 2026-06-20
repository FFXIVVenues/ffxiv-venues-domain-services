using Discord.WebSocket;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;

namespace FFXIVVenues.Veni.Infrastructure.Context.Abstractions
{
    public interface ISessionProvider
    {
        Session GetSession(SocketMessage message);

        Session GetSession(SocketMessageComponent message);

        Session GetSession(SocketSlashCommand message);
            
        Session GetSession(string key);
    }
}