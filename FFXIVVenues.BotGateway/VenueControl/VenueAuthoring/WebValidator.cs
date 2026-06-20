using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels;
using Serilog;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring;

public interface ISiteValidator
{
    Task<SiteCheckResult> CheckUrlAsync(Venue venue);
}

public class SiteValidator(HttpClient client) : ISiteValidator
{

    private RollingCache<SiteCheckResult> _cache = new(TimeSpan.FromMinutes(5), TimeSpan.FromHours(1));
    
    public async Task<SiteCheckResult> CheckUrlAsync(Venue venue)
    {
        if (venue.Website is null)
            return SiteCheckResult.Unset;
        
        var cached = _cache.Get(venue.Website.ToString());
        if (cached.Result is CacheResult.CacheHit)
            return cached.Value;

        try
        {
            var response = await client.GetAsync(venue.Website, HttpCompletionOption.ResponseHeadersRead);
            if (response.IsSuccessStatusCode || response.StatusCode is HttpStatusCode.TooManyRequests)
            {
                _cache.Set(venue.Website.ToString(), SiteCheckResult.Valid);
                return SiteCheckResult.Valid;
            }

            Log.Debug("{Venue} has invalid site Url ({Url}). Status code was {StatusCode}.", venue, venue.Website,
                response.StatusCode);
        }
        catch (Exception e)
        {
            Log.Debug(e, "{VenueId} has invalid site Url ({Url}).", venue.Id, venue.Website);
        }
        _cache.Set(venue.Website.ToString(), SiteCheckResult.Invalid);
        return SiteCheckResult.Invalid;
    }
}

public enum SiteCheckResult
{
    Valid,
    Invalid,
    Unset
}