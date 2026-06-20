using Discord.WebSocket;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;

namespace FFXIVVenues.Veni.Infrastructure.Context
{
    public interface IVeniInteractionContext
    {
        DiscordSocketClient Client { get; }
        Session Session { get; }

    }
}