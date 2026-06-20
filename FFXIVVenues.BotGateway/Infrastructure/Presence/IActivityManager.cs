using System.Threading.Tasks;

namespace FFXIVVenues.BotGateway.Infrastructure.Presence;

public interface IActivityManager
{
    Task UpdateActivityAsync();
}