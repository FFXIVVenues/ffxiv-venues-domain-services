using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;

namespace FFXIVVenues.Veni.Infrastructure.Commands
{
    public interface ICommandHandler
    {
        Task HandleAsync(SlashCommandVeniInteractionContext slashCommand);
    }

}
