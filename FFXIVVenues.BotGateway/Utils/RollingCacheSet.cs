using System;
using System.Collections.Concurrent;

namespace FFXIVVenues.Veni.Utils;

public class RollingCacheSet
{
    private ConcurrentDictionary<Type, object> _caches = new();

    public RollingCache<T> For<T>(long timeoutInMs = 60_000, long maxAgeInMs = 300_000) => 
        (RollingCache<T>) this._caches.GetOrAdd(typeof(T), 
            _ => new RollingCache<T>(timeoutInMs, maxAgeInMs));
}