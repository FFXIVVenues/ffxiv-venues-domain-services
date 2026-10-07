using Microsoft.Extensions.DependencyInjection;

namespace FFXIVVenues.DomainSecurity;

public static class ServicePoint
{
    public static IServiceCollection AddSecurityServices(this IServiceCollection services, Action<SecurityOptions> configure)
    {
        services.Configure(configure);
        services.AddSingleton<SecurityKeyLoader>();
        services.AddSingleton<Signer>();
        return services;
    }
}


public class SecurityOptions
{
    public string SigningPrivateKeyPath { get; set; } = "config/private.pem";
    public string SigningPublicKeyPath { get; set; } = "config/public.pub";
}