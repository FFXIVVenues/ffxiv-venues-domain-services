using Discord.WebSocket;

namespace FFXIVVenues.BotGateway.Infrastructure.Context;

public interface IInteractionContextFactory
{

    MessageVeniInteractionContext Create(SocketMessage message);
    ComponentVeniInteractionContext Create(SocketMessageComponent message);
    SlashCommandVeniInteractionContext Create(SocketSlashCommand message);

}