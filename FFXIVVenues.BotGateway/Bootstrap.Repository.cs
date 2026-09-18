using FFXIVVenues.BotGateway.Infrastructure.Persistence;
using FFXIVVenues.BotGateway.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.DomainData;
using Microsoft.Extensions.DependencyInjection;

namespace FFXIVVenues.Veni;

internal static partial class Bootstrap
{
    internal static void ConfigureData(IServiceCollection serviceCollection, Configurations config)
    {
        IRepository repository = config.PersistenceConfig.Provider switch
        {
            PersistanceProvider.LiteDb => new LiteDbRepository(config.PersistenceConfig.ConnectionString),
            PersistanceProvider.MongoDb => new MongoDbRepository(config.PersistenceConfig.ConnectionString),
            _ => new InMemoryRepository()
        };
        serviceCollection.AddSingleton(repository);
        serviceCollection.AddDomainData(config.ConnectionString, config.UiConfig.UriTemplate);
    }
}