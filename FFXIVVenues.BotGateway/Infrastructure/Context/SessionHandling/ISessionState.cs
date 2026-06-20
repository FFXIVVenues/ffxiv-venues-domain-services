using FFXIVVenues.BotGateway.Infrastructure.Context;
using System.Threading.Tasks;

namespace FFXIVVenues.BotGateway.Infrastructure.Context.SessionHandling
{
    public interface ISessionState
    {
        Task Enter(VeniInteractionContext c);
    }
}