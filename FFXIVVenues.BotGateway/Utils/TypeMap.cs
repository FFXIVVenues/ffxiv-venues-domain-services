using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;

namespace FFXIVVenues.BotGateway.Utils;

internal class TypeMap<T>(IServiceProvider serviceProvider) : IEnumerable<T>
    where T : class
{

    private readonly Dictionary<string, Type> _typeMap = new();

    public TypeMap<T> Add<A>(string key) where A : T
    {
        _typeMap.Add(key, typeof(A));
        return this;
    }

    public TypeMap<T> Add(string key, Type type)
    {
        _typeMap.Add(key, type);
        return this;
    }
        
    public Type Get(string key)
    {
        return _typeMap[key];
    }

    public T Activate(string key, ActivateResolve scope = ActivateResolve.KeepServiceScope)
    {
        var hasKey = _typeMap.ContainsKey(key);
        if (!hasKey)
        {
            return default;
        }

        var sp = serviceProvider;
        if (scope == ActivateResolve.NewServiceScope)
            sp = serviceProvider.CreateScope().ServiceProvider;
        
        return ActivatorUtilities.CreateInstance(sp, _typeMap[key]) as T;
    }

    public IEnumerator<T> GetEnumerator()
    {
        foreach (var key in this._typeMap.Keys)
            yield return this.Activate(key);
    }

    IEnumerator IEnumerable.GetEnumerator() =>
        this.GetEnumerator();

}

public enum ActivateResolve
{
    KeepServiceScope,
    NewServiceScope
}