using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;

namespace FFXIVVenues.Veni;

internal static partial class Bootstrap
{
    internal static void ConfigureApiClient(IServiceCollection serviceCollection, Configurations config)
    {
        var apiHttpClient = new HttpClient { BaseAddress = new Uri(config.ApiConfig.BaseUrl) };
        apiHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.ApiConfig.AuthorizationKey);
        apiHttpClient.Timeout = TimeSpan.FromSeconds(10);
        serviceCollection.AddSingleton(apiHttpClient);
    }
}