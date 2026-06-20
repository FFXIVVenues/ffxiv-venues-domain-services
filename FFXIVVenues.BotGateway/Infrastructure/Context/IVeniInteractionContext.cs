using Discord.WebSocket;
using FFXIVVenues.BotGateway.Infrastructure.Context.SessionHandling;

namespace FFXIVVenues.BotGateway.Infrastructure.Context
{
    public interface IVeniInteractionContext
    {
        DiscordSocketClient Client { get; }
        Session Session { get; }

    }
}