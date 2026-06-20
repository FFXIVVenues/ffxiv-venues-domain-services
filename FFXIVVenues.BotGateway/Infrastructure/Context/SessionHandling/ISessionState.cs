using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Infrastructure.Context.SessionHandling
{
    public interface ISessionState
    {
        Task Enter(VeniInteractionContext c);
    }
}