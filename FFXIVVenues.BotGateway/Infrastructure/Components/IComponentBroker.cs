using System.Threading.Tasks;
using Discord.WebSocket;
using FFXIVVenues.BotGateway.Infrastructure.Context;

namespace FFXIVVenues.BotGateway.Infrastructure.Components;

public interface IComponentBroker
{
    void Add<THandler>(string key) where THandler : IComponentHandler;
    Task HandleAsync(ComponentVeniInteractionContext component);
}