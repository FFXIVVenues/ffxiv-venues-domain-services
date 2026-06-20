using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Authorisation.Blacklist;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.Utils;
using MongoDB.Driver;
using Serilog;

namespace FFXIVVenues.Veni.Infrastructure.Persistence;

public class MongoDbRepository : IRepository, IDisposable
{
    private readonly IMongoDatabase _database;
    private readonly RollingCacheSet _cache = new ();

    public MongoDbRepository(string strConnectionString)
    {
        var mongoConnectonString = new MongoUrl(strConnectionString);
        var client = new MongoClient(mongoConnectonString);
        this._database = client.GetDatabase(mongoConnectonString.DatabaseName  ?? "ffxivvenues-veni");
        this._cache.For<BlacklistEntry>(3*60*60*1000, 3*60*60*1000);
    }

    public async Task UpsertAsync<T>(T entity) where T : class, IEntity
    {
        var typeName = typeof(T).Name;
        Log.Debug("Upserting {EntityType} {EntityId}", typeName, entity.id);
        this._cache.For<T>().Remove(entity.id);
        var collection = this._database.GetCollection<T>(typeName);
        var filter = Builders<T>.Filter.Eq("id", entity.id);
        await collection.ReplaceOneAsync(filter, entity, new ReplaceOptions { IsUpsert = true });
    }

    public Task DeleteAsync<T>(T entity) where T : class, IEntity =>
        this.DeleteAsync<T>(entity.id);

    public async Task DeleteAsync<T>(string id) where T : class, IEntity
    {
        var typeName = typeof(T).Name;
        Log.Debug("Deleting {EntityType} {EntityId}", typeName, id);
        this._cache.For<T>().Remove(id);
        var collection = this._database.GetCollection<T>(typeName);
        var filter = Builders<T>.Filter.Eq("id", id);
        await collection.DeleteOneAsync(filter);
    }

    public Task<IQueryable<T>> GetWhereAsync<T>(Expression<Func<T, bool>> predicate) where T : class, IEntity
    {
        var typeName = typeof(T).Name;
        Log.Debug("Getting {EntityType} where {EntityQuery}", typeName, predicate);
        var collection = this._database.GetCollection<T>(typeName);
        return Task.FromResult(collection.AsQueryable().Where(predicate));
    }

    public Task<IQueryable<T>> QueryAsync<T>() where T : class, IEntity
    {
        var typeName = typeof(T).Name;
        Log.Debug("Getting all {EntityType}", typeName);
        var collection = this._database.GetCollection<T>(typeName);
        return Task.FromResult<IQueryable<T>>(collection.AsQueryable());
    }

    public async Task<T> GetByIdAsync<T>(string id) where T : class, IEntity
    {
        var typeName = typeof(T).Name;
        var cacheResult = this._cache.For<T>().Get(id);
        Log.Debug("Getting {EntityType} {EntityId} ({Cache})", typeName, id, cacheResult.Result);
        if (cacheResult.Result == CacheResult.CacheHit)
            return cacheResult.Value;

        var collection = this._database.GetCollection<T>(typeName);
        var filter = Builders<T>.Filter.Eq("id", id);
        var results = await collection.FindAsync(filter);
        var result = results.FirstOrDefault();
        this._cache.For<T>().Set(id, result);
        return result;
    }

    public Task<bool> ExistsAsync<T>(string id) where T : class, IEntity =>
        this.GetByIdAsync<T>(id).ContinueWith(e => e.IsCompletedSuccessfully && e.Result != null);

    public void Dispose() =>
        GC.SuppressFinalize(this);

    ~MongoDbRepository() =>
        Dispose();

}