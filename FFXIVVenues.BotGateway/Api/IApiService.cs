using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.Api
{
    public interface IApiService
    {
        Task<IEnumerable<Venue>> GetAllVenuesAsync();
        Task<IEnumerable<Venue>> GetAllVenuesAsync(ulong forContact);
        Task<IEnumerable<Venue>> GetAllVenuesAsync(string searchTerm);
        Task<IEnumerable<Venue>> GetApprovedVenuesAsync();
        Task<IEnumerable<Venue>> GetUnapprovedVenuesAsync();
        Task<IEnumerable<Venue>> GetOpenVenuesAsync();
        Task<Venue> GetVenueAsync(string id, bool recordView = false);
        Task<HttpResponseMessage> PutVenueAsync(Venue venue);
        Task<HttpResponseMessage> PutVenueBannerAsync(string id, string url);
        Task<HttpResponseMessage> PutVenueBannerAsync(string id, Stream stream, MediaTypeHeaderValue mediaType);
        Task<HttpResponseMessage> DeleteVenueAsync(string id);
        Task<HttpResponseMessage> OpenVenueAsync(string id, DateTimeOffset from, DateTimeOffset to);
        Task<HttpResponseMessage> CloseVenueAsync(string id, DateTimeOffset from, DateTimeOffset to);
        Task<HttpResponseMessage> RemoveOverridesAsync(string id, DateTimeOffset from, DateTimeOffset to);
        Task<HttpResponseMessage> ApproveAsync(string id, bool approval = true);
    }
}