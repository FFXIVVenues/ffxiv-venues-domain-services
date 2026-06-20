namespace FFXIVVenues.Veni.Infrastructure.Persistence;

public class PersistenceConfiguration
{
    public PersistanceProvider Provider { get; set; }
    public string ConnectionString { get; set; }
}

public enum PersistanceProvider
{
    InMemory,
    LiteDb,
    Cosmos,
    MongoDb
}