using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Discord.WebSocket;
using FFXIVVenues.Veni.Infrastructure.Context;

namespace FFXIVVenues.Veni.Infrastructure.Components;

public interface IComponentHandler
{
    Task HandleAsync(ComponentVeniInteractionContext context, string[] args);
}