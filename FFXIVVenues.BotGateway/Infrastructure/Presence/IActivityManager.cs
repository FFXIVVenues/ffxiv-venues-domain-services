using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Infrastructure.Presence;

public interface IActivityManager
{
    Task UpdateActivityAsync();
}