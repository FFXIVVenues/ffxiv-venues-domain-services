using Discord.WebSocket;
using FFXIVVenues.Veni.Infrastructure.Context.Abstractions;


namespace FFXIVVenues.Veni.Infrastructure.Context;

public class InteractionContextFactory : IInteractionContextFactory
{
    private readonly ISessionProvider _sessionProvider;
    private readonly DiscordSocketClient _client;

    public InteractionContextFactory(ISessionProvider sessionProvider, DiscordSocketClient client)
    {
        this._sessionProvider = sessionProvider;
        this._client = client;
    }

    public MessageVeniInteractionContext Create(SocketMessage message)
    {
        var conversationContext = _sessionProvider.GetSession(message);
        return new MessageVeniInteractionContext(message, this._client, conversationContext);
    }

    public ComponentVeniInteractionContext Create(SocketMessageComponent message)
    {
        var session = _sessionProvider.GetSession(message);
        return new ComponentVeniInteractionContext(message, _client, session);
    }

    public SlashCommandVeniInteractionContext Create(SocketSlashCommand message)
    {
        var sessionContext = _sessionProvider.GetSession(message);
        return new SlashCommandVeniInteractionContext(message, _client, sessionContext);
    }
    
}