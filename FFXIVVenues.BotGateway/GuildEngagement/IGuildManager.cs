using System.Threading.Tasks;
using Discord;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.BotGateway.GuildEngagement
{
    public interface IGuildManager
    {
        Task<bool> AssignRolesForVenueAsync(Venue venue);

        Task<bool> SyncRolesForVenueAsync(Venue venue);

        Task<bool> SyncRolesForGuildUserAsync(IGuildUser user, GuildSettings guildSettings = null);

        Task<bool> FormatDisplayNamesForVenueAsync(Venue venue);

        Task<bool> FormatDisplayNameForGuildUserAsync(IGuildUser user, GuildSettings guildSettings = null);
        
        Task<bool> WelcomeGuildUserAsync(IGuildUser user);
    }
}