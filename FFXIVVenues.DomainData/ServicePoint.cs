using FFXIVVenues.DomainData.Context;
using FFXIVVenues.DomainData.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FFXIVVenues.DomainData;

public static class ServicePoint
{
    
    public static IServiceCollection AddDomainData(this IServiceCollection services, 
        string connectionString, 
        string mediaUriTemplate = null)
    {
        services.AddScoped<DomainDataContext>();
        services.AddSingleton(new DbContextOptionsBuilder<DomainDataContext>().UseNpgsql(connectionString).Options);
        services.AddSingleton(new DomainDataConnectionString(connectionString));
        if (mediaUriTemplate is not null)
            services.AddSingleton<IMapFactory>(new MapFactory(new(mediaUriTemplate)));
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