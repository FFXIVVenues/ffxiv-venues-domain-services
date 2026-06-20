using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Api;

namespace FFXIVVenues.Veni.Infrastructure.Presence;

public class ActivityManager(DiscordSocketClient client, PresenceConfiguration config, IApiService venuesApi) : IActivityManager
{
    public async Task UpdateActivityAsync()
    {
        var allVenuesTask = venuesApi.GetAllVenuesAsync();
        var allVenues = await allVenuesTask;
        var countOfVenues = allVenues.Count();
        var activity = config.Activity.Replace("{count}", countOfVenues.ToString());
        await client.SetActivityAsync(new Game(activity));
    }
}