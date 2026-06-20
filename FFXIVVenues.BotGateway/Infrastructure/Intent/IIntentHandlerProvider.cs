using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;

namespace FFXIVVenues.Veni.Infrastructure.Intent
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