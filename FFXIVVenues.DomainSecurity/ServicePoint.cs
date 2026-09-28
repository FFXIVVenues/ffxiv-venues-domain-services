using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace FFXIVVenues.DomainSecurity;

public static class ServicePoint
{
    public static IServiceCollection AddSecurityServices(this IServiceCollection services, Action<SecurityOptions> configure)
    {
        var options = new SecurityOptions();
        configure(options);

        services.AddSingleton(new SecurityKeyLoader(options));
        services.AddSingleton(i => new Signer(i.GetRequiredService<SecurityKeyLoader>()));
        return services;
    }
}

public class SecurityOptions
{
    public string SigningPrivateKeyPath { get; set; } = "config/private.pem";
    public string SigningPublicKeyPath { get; set; } = "config/public.pub";
}