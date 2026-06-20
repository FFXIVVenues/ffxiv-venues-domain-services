using System.Threading.Tasks;
using FFXIVVenues.BotGateway.Infrastructure.Context;

namespace FFXIVVenues.BotGateway.Infrastructure.Intent
{
    public interface IIntentHandlerProvider
    {
        Task HandleIteruptIntent(string interupt, MessageVeniInteractionContext context);

        Task HandleIteruptIntent(string interupt, ComponentVeniInteractionContext context);

        Task HandleIteruptIntent(string interupt, SlashCommandVeniInteractionContext context);

        Task HandleIntent(string interupt, MessageVeniInteractionContext context);

        Task HandleIntent(string interupt, ComponentVeniInteractionContext context);

        Task HandleIntent(string interupt, SlashCommandVeniInteractionContext context);
    }
}