using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FFXIVVenues.BotGateway.Utils;
using FFXIVVenues.VenueModels;
using Serilog;

namespace FFXIVVenues.BotGateway.VenueControl.VenueAuthoring;

public class SiteValidator(HttpClient client)
{

    private readonly RollingCache<SiteCheckResult> _cache = new(TimeSpan.FromMinutes(5), TimeSpan.FromHours(1));

    public Task<SiteCheckResult> CheckUrlAsync(Uri website) =>
        this.CheckUrlAsync(website?.ToString());
    
    public async Task<SiteCheckResult> CheckUrlAsync(string website)
    {
        if (website is null)
            return SiteCheckResult.Unset;
        
        var cached = _cache.Get(website);
        if (cached.Result is CacheResult.CacheHit)
            return cached.Value;

        try
        {
            var response = await client.GetAsync(website, HttpCompletionOption.ResponseHeadersRead);
            if (response.IsSuccessStatusCode || response.StatusCode is HttpStatusCode.TooManyRequests)
            {
                _cache.Set(website, SiteCheckResult.Valid);
                return SiteCheckResult.Valid;
            }

            Log.Debug("{Url} is invalid site Url. Status code was {StatusCode}", website, response.StatusCode);
        }
        catch (Exception e)
        {
            Log.Debug(e, "{Url} is invalid site Url", website);
        }
        _cache.Set(website, SiteCheckResult.Invalid);
        return SiteCheckResult.Invalid;
    }
}

public enum SiteCheckResult
{
    Valid,
    Invalid,
    Unset
}