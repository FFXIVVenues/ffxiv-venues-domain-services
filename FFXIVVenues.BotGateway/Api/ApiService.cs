using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels;

using Serilog;

namespace FFXIVVenues.Veni.Api;

internal class ApiService(HttpClient httpClient) : IApiService
{
    private readonly RollingCache<Venue> _venueCache = new(60*1000, 10 * 60 * 1_000);
    private readonly RollingCache<Venue[]> _venuesCache = new(60*1000, 10 * 60 * 1_000);

    public async Task<IEnumerable<Venue>> GetAllVenuesAsync()
    {
        var cached = this._venuesCache.Get("*");
        Log.Debug("Getting all venues ({Cache})", cached.Result);
        if (cached.Result == CacheResult.CacheHit)
            return cached.Value;
            
        var response = await httpClient.GetAsync($"/v1.0/venue");
        var result = await response.Content.ReadFromJsonAsync<Venue[]>();
        this._venuesCache.Set("*", result);    
        return result;
    }

    public async Task<IEnumerable<Venue>> GetAllVenuesAsync(ulong forContact)
    {
        var cached = this._venuesCache.Get(forContact.ToString());
        Log.Debug("Getting all venues for contact {Contact} ({Cache})", forContact, cached.Result);
        if (cached.Result == CacheResult.CacheHit)
            return cached.Value;
            
        var response = await httpClient.GetAsync($"/v1.0/venue?manager={forContact}");
        var result = await response.Content.ReadFromJsonAsync<Venue[]>();
        this._venuesCache.Set(forContact.ToString(), result);
        return result;
    }

    public async Task<IEnumerable<Venue>> GetOpenVenuesAsync()
    {
        var cached = this._venuesCache.Get("_open_");
        Log.Debug("Getting all open venues ({Cache})", cached.Result);
        if (cached.Result == CacheResult.CacheHit)
            return cached.Value;
            
        var response = await httpClient.GetAsync($"/v1.0/venue?open=true");
        var result = await response.Content.ReadFromJsonAsync<Venue[]>();
        this._venuesCache.Set("_open_", result);
        return result;
    }

    public async Task<IEnumerable<Venue>> GetApprovedVenuesAsync()
    {
        var cached = this._venuesCache.Get("_approved_");
        Log.Debug("Getting all approved venues ({Cache})", cached.Result);
        if (cached.Result == CacheResult.CacheHit)
            return cached.Value;
            
        var response = await httpClient.GetAsync($"/v1.0/venue?approved=true");
        var result = await response.Content.ReadFromJsonAsync<Venue[]>();
        this._venuesCache.Set("_approved_", result);
        return result;
    }
        
    public async Task<IEnumerable<Venue>> GetUnapprovedVenuesAsync()
    {
        var cached = this._venuesCache.Get("_unapproved_");
        Log.Debug("Getting all unapproved venues ({Cache})", cached.Result);
        if (cached.Result == CacheResult.CacheHit)
            return cached.Value;
            
        var response = await httpClient.GetAsync($"/v1.0/venue?approved=false");
        var result = await response.Content.ReadFromJsonAsync<Venue[]>();
        this._venuesCache.Set("_unapproved_", result);
        return result;
    }

    public async Task<IEnumerable<Venue>> GetAllVenuesAsync(string searchQuery)
    {
        var cached = this._venuesCache.Get($"_search_{searchQuery}");
        Log.Debug("Getting all venues matching search '{SearchQuery}' ({Cache})", searchQuery, cached.Result);
        if (cached.Result == CacheResult.CacheHit)
            return cached.Value;
            
        var response = await httpClient.GetAsync($"/v1.0/venue?search={searchQuery}");
        var result = await response.Content.ReadFromJsonAsync<Venue[]>();
        this._venuesCache.Set($"_search_{searchQuery}", result);
        return result;
    }

    public async Task<Venue> GetVenueAsync(string id, bool recordView = false)
    {
        var cached = this._venueCache.Get(id);
        Log.Debug("Getting venue {VenueId} ({Cache})", id, cached.Result);
        if (cached.Result == CacheResult.CacheHit)
            return cached.Value;

        var response = await httpClient.GetAsync("/v1.0/venue/" + id + "?recordView=" + recordView);
        var result = await response.Content.ReadFromJsonAsync<Venue>();
        this._venueCache.Set(id, result);
        return result;
    }

    public async Task<HttpResponseMessage> PutVenueAsync(Venue venue)
    {
        Log.Debug("Putting venue {Venue}", venue);
        this._venueCache.Remove(venue.Id);
        this._venuesCache.Clear();
        var response = await httpClient.PutAsJsonAsync("/v1.0/venue/" + venue.Id, venue);
        if (!response.IsSuccessStatusCode)
            Log.Warning("Failed to put venue {Venue}: {ResponseStatusCode}", venue, response.StatusCode);
        return response;
    }

    public async Task<HttpResponseMessage> PutVenueBannerAsync(string id, string url)
    {
        var response = await httpClient.GetAsync(url);
        var stream = await response.Content.ReadAsStreamAsync();
        this._venueCache.Remove(id);
        this._venuesCache.Clear();
        return await PutVenueBannerAsync(id, stream, response.Content.Headers.ContentType);
    }

    public async Task<HttpResponseMessage> PutVenueBannerAsync(string id, Stream stream, MediaTypeHeaderValue mediaType)
    {
        Log.Debug("Putting splash banner for venue {VenueId}", id);
        var streamContent = new StreamContent(stream);
        streamContent.Headers.ContentType = mediaType;
        this._venueCache.Remove(id);
        this._venuesCache.Clear();
        var response = await httpClient.PutAsync("/v1.0/venue/" + id + "/media", streamContent);
        if (!response.IsSuccessStatusCode)
            Log.Warning("Failed to put splash banner for venue {VenueId}: {ResponseStatusCode}", id, response.StatusCode);
        return response;
    }

    public async Task<HttpResponseMessage> DeleteVenueAsync(string id)
    {
        Log.Debug("Deleting venue {VenueId}", id);
        this._venueCache.Remove(id);
        this._venuesCache.Clear();
        var response = await httpClient.DeleteAsync("/v1.0/venue/" + id);
        if (!response.IsSuccessStatusCode)
            Log.Warning("Failed to delete venue {VenueId}: {ResponseStatusCode}", id, response.StatusCode);
        return response;
    }

    public async Task<HttpResponseMessage> OpenVenueAsync(string id, DateTimeOffset from, DateTimeOffset to)
    {
        Log.Debug("Opening venue {VenueId}", id);
        this._venueCache.Remove(id);
        this._venuesCache.Clear();
        var @override = new ScheduleOverride { Open = true, Start = from, End = to };
        var response = await httpClient.PutAsJsonAsync($"/v1.0/venue/{id}/scheduleoverride", @override);
        if (!response.IsSuccessStatusCode)
            Log.Warning("Failed to open venue {VenueId}: {ResponseStatusCode}", id, response.StatusCode);
        return response;
    }

    public async Task<HttpResponseMessage> CloseVenueAsync(string id, DateTimeOffset from, DateTimeOffset to)
    {
        Log.Debug("Closing venue {VenueId}", id);
        this._venueCache.Remove(id);
        this._venuesCache.Clear();
        var @override = new ScheduleOverride { Open = false, Start = from, End = to };
        var response = await httpClient.PutAsJsonAsync($"/v1.0/venue/{id}/scheduleoverride", @override);
        if (!response.IsSuccessStatusCode)
            Log.Warning("Failed to close venue {VenueId}: {ResponseStatusCode}", id, response.StatusCode);
        return response;
    }
    
    public async Task<HttpResponseMessage> RemoveOverridesAsync(string id, DateTimeOffset from, DateTimeOffset to)
    {
        from = from.ToUniversalTime();
        to = to.ToUniversalTime();
        Log.Debug("Removing overrides from venue {VenueId}", id);
        this._venueCache.Remove(id);
        this._venuesCache.Clear();
        var response = await httpClient.DeleteAsync($"/v1.0/venue/{id}/scheduleoverride?from={from:yyyy-MM-ddTHH:mm:ss.fffZ}&to={to:yyyy-MM-ddTHH:mm:ss.fffZ}");
        if (!response.IsSuccessStatusCode)
            Log.Warning("Failed to close venue {VenueId}: {ResponseStatusCode}", id, response.StatusCode);
        return response;
    }

    public async Task<HttpResponseMessage> ApproveAsync(string id, bool approval = true)
    {
        if (approval) Log.Debug("Approving venue {VenueId}", id);
        else Log.Debug("Unapproving venue {VenueId}", id);
        this._venueCache.Remove(id);
        this._venuesCache.Clear();
        var response = await httpClient.PutAsJsonAsync($"/v1.0/venue/{id}/approved", approval);
        if (!response.IsSuccessStatusCode)
            Log.Warning("Failed to approve/unapprove venue {VenueId}: {ResponseStatusCode}", id, response.StatusCode);
        return response;
    }

}