using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using IPNetwork = System.Net.IPNetwork;

namespace FFXIVVenues.ApiGateway.Bootstrap;

internal static partial class Bootstrap {
    public static async Task<IApplicationBuilder> ConfigureForwardHeaders(this WebApplication app, IConfigurationSection config) {
        Log.Information("Configuring forward headers");
        var sources = config.GetSection("Sources").Get<List<string>>();
        var cidrs = config.GetSection("CIDRs").Get<List<string>>();
        var ips = config.GetSection("IPs").Get<List<string>>();
        
        var forwardedHeadersOptions = new ForwardedHeadersOptions();
        forwardedHeadersOptions.ForwardedHeaders = ForwardedHeaders.XForwardedFor;
        forwardedHeadersOptions.KnownIPNetworks.Clear();
        forwardedHeadersOptions.KnownProxies.Clear();
        forwardedHeadersOptions.ForwardLimit = null;

        if (cidrs is { Count: > 0 })
            foreach (var cidr in cidrs)
                if (IPNetwork.TryParse(cidr, out var network))
                    forwardedHeadersOptions.KnownIPNetworks.Add(network);

        if (ips is { Count: > 0 })
            foreach (var ip in ips)
                if (IPAddress.TryParse(ip, out var address))
                    forwardedHeadersOptions.KnownProxies.Add(address);

        if (sources is { Count: > 0 })
        {
            var httpClient = app.Services.GetRequiredService<IHttpClientFactory>().CreateClient();
            foreach (var source in sources)
            {
                var response = await httpClient.GetStringAsync(source);
                foreach (var line in response.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    if (IPNetwork.TryParse(line, out var network))
                        forwardedHeadersOptions.KnownIPNetworks.Add(network);
                    else if (IPAddress.TryParse(line, out var address))
                        forwardedHeadersOptions.KnownProxies.Add(address);
                }
            }
        }

        app.UseForwardedHeaders(forwardedHeadersOptions);
        Log.Information("Forward headers configured");
        return app;
    }
}