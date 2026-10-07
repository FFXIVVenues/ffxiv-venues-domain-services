using FFXIVVenues.DomainData.Context;
using FFXIVVenues.DomainData.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FFXIVVenues.DomainData;

public static class ServicePoint
{
    
    public static IServiceCollection AddDomainData(this IServiceCollection services, 
        Action<DomainDataConfiguration> configure)
    {
        services.Configure(configure);
        services.AddDbContextFactory<DomainDataContext>((sp, c) => 
            c.UseNpgsql(sp.GetRequiredService<IOptions<DomainDataConfiguration>>().Value.ConnectionString));
        services.AddSingleton<IMapFactory, MapFactory>();
        return services;
    }

    public static async Task MigrateDomainDataAsync(this IServiceProvider provider)
    {
        await using var scope = provider.CreateAsyncScope();
        await using var db = scope.ServiceProvider.GetService<DomainDataContext>();
        if (db == null) throw new Exception("No database context available.");
        await db.Database.MigrateAsync();
    }
}