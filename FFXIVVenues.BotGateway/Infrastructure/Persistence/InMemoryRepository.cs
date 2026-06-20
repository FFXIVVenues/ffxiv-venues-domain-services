using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;

namespace FFXIVVenues.Veni.Infrastructure.Persistence;

public class InMemoryRepository : IRepository
{

    private ConcurrentDictionary<Type, ConcurrentDictionary<string, IEntity>> _memoryStore = new();

    public Task UpsertAsync<T>(T entity) where T : class, IEntity
    {
        var type = typeof(T);
        if (!_memoryStore.ContainsKey(type))
        {
            _memoryStore[type] = new ConcurrentDictionary<string, IEntity>();
        }
        _memoryStore[type][entity.id] = entity;
        return Task.CompletedTask;
    }

    public Task DeleteAsync<T>(T entity) where T : class, IEntity
    {
        this.DeleteAsync<T>(entity.id);
        return Task.CompletedTask;
    }

    public Task DeleteAsync<T>(string id) where T : class, IEntity
    {
        var type = typeof(T);
        if (_memoryStore.ContainsKey(type))
            _memoryStore[type].TryRemove(id, out _);
        return Task.CompletedTask;
    }

    public Task<IQueryable<T>> GetWhereAsync<T>(Expression<Func<T, bool>> predicate) where T : class, IEntity
    {
        var type = typeof(T);
        if (!_memoryStore.ContainsKey(type))
        {
            return Task.FromResult(Enumerable.Empty<T>().AsQueryable());
        }

        var func = predicate.Compile();
        return Task.FromResult(_memoryStore[type].Values
            .Select(item => item as T)
            .Where(item => item != null && func(item))
            .AsQueryable()!);
    }


    public Task<IQueryable<T>> QueryAsync<T>() where T : class, IEntity
    {
        var type = typeof(T);
        if (!_memoryStore.ContainsKey(type))
        {
            return Task.FromResult(Enumerable.Empty<T>().AsQueryable());
        }

        return Task.FromResult(_memoryStore[type].Values
            .Select(item => item as T)
            .Where(item => item != null)
            .AsQueryable()!);
    }

    public Task<T> GetByIdAsync<T>(string id) where T : class, IEntity
    {
        var type = typeof(T);
        if (!_memoryStore.ContainsKey(type))
        {
            return Task.FromResult<T>(null);
        }

        return Task.FromResult(_memoryStore[type].TryGetValue(id, out var value) ? value as T : null);
    }

    public Task<bool> ExistsAsync<T>(string id) where T : class, IEntity
    {
        var type = typeof(T);
        if (!_memoryStore.ContainsKey(type))
        {
            return Task.FromResult(false);
        }
        return Task.FromResult(_memoryStore[type].ContainsKey(id));
    }

}