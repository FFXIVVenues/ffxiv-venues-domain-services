using System;
using System.Net.Http;
using System.Net.Http.Headers;
using FFXIVVenues.Veni.Infrastructure.Persistence;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FFXIVVenues.Veni;

internal static partial class Bootstrap
{
    internal static void ConfigureRepository(IServiceCollection serviceCollection, Configurations config)
    {
        IRepository repository = config.PersistenceConfig.Provider switch
        {
            PersistanceProvider.LiteDb => new LiteDbRepository(config.PersistenceConfig.ConnectionString),
            PersistanceProvider.MongoDb => new MongoDbRepository(config.PersistenceConfig.ConnectionString),
            _ => new InMemoryRepository()
        };
        serviceCollection.AddSingleton(repository);
    }
}