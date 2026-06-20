using System.Threading.Tasks;
using FFXIVVenues.BotGateway.Infrastructure.Context;

namespace FFXIVVenues.BotGateway.Infrastructure.Commands
{
    public interface ICommandHandler
    {
        Task HandleAsync(SlashCommandVeniInteractionContext slashCommand);
    }

}
