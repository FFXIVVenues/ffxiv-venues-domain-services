using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels;
using Serilog;
using CheckReturn = (FFXIVVenues.Veni.VenueControl.VenueAuthoring.DiscordCheckResult Result, Discord.Rest.RestInviteMetadata Invite);

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring;

public interface IDiscordValidator
{
    Task<CheckReturn> CheckInviteAsync(Uri rawDiscordString);
    
    Task<CheckReturn> CheckInviteAsync(Venue venue);
}

public class DiscordValidator(DiscordSocketClient client) : IDiscordValidator
{
    
    private RollingCache<CheckReturn> _cache = new(TimeSpan.FromMinutes(5), TimeSpan.FromHours(1));
    private static readonly Regex _discordPattern = new(@"^(https?:\/\/)?(www\.)?((discord(app)?(\.com|\.io)(\/invite)?)|(discord\.gg))\/([\w-]+)$");
    
    public async Task<CheckReturn> CheckInviteAsync(Uri rawDiscordString)
    {
        CheckReturn result;
        
        if (rawDiscordString is null)
            return (DiscordCheckResult.Unset, null);
        
        var cached = _cache.Get(rawDiscordString.ToString());
        if (cached.Result is CacheResult.CacheHit)
            return cached.Value;

        var match = _discordPattern.Match(rawDiscordString.ToString());
        if (!match.Success)
        {
            result = (DiscordCheckResult.BadFormat, null);
            _cache.Set(rawDiscordString.ToString(), result);
            return result;
        }

        var inviteCode = match.Groups[9].ToString();
        var invite = await client.GetInviteAsync(inviteCode, new RequestOptions());
        
        if (invite is null)
            result = (DiscordCheckResult.InvalidInvite, null);
        else if (invite.ExpiresAt is not null)
            result = (DiscordCheckResult.Expires, invite);
        else
            result = (DiscordCheckResult.Valid, invite);
        
        _cache.Set(rawDiscordString.ToString(), result);
        return result;
    }

    public async Task<CheckReturn> CheckInviteAsync(Venue venue)
    {
        var result = await this.CheckInviteAsync(venue.Discord);
        if (result.Result is not DiscordCheckResult.Valid and not DiscordCheckResult.Unset)
            Log.Debug("{VenueId} has invalid discord invite ({Url}); {Status}.", venue.Id, venue.Discord, result.Result);

        return result;
    }
}

public enum DiscordCheckResult
{
    Valid,
    BadFormat,
    InvalidInvite,
    Expires,
    Unset
}