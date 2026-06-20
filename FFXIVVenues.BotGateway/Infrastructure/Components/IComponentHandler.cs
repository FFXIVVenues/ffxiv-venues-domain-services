using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Discord.WebSocket;
using FFXIVVenues.BotGateway.Infrastructure.Context;

namespace FFXIVVenues.BotGateway.Infrastructure.Components;

public interface IComponentHandler
{
    Task HandleAsync(ComponentVeniInteractionContext context, string[] args);
}