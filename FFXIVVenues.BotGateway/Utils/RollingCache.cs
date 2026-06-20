using System;
using System.Collections.Concurrent;
using System.Threading;

namespace FFXIVVenues.Veni.Utils
{
    public class RollingCache<T> : IDisposable
    {
        public double TimeoutInMs { get; }
        public double MaxAgeInMs { get; }

        private readonly ConcurrentDictionary<string, DateTime> _set = new();
        private readonly ConcurrentDictionary<string, DateTime> _lastAccess = new();
        private readonly ConcurrentDictionary<string, T> _items = new();
        private readonly Timer _timer;
        private bool _disposed;

        public RollingCache(long timeoutInMs, long maxAgeInMs)
        {
            this.TimeoutInMs = timeoutInMs;
            this.MaxAgeInMs = maxAgeInMs;
            this._timer = new Timer(_ => ExpireItems(), null, 60_000, 60_000);
        }
        
        public RollingCache(TimeSpan timeoutInMs, TimeSpan maxAgeInMs)
        {
            this.TimeoutInMs = timeoutInMs.TotalMilliseconds;
            this.MaxAgeInMs = maxAgeInMs.TotalMilliseconds;
            this._timer = new Timer(_ => ExpireItems(), null, 60_000, 60_000);
        }

        public CacheResult<T> Get(string key)
        {
            var exists = _items.TryGetValue(key, out var value);
            if (exists)
            {
                _lastAccess[key] = DateTime.UtcNow;
                return new (CacheResult.CacheHit, value);
            }
            return new (CacheResult.CacheMiss, default);
        }

        public void Set(string key, T value)
        {
            _lastAccess[key] = DateTime.UtcNow;
            _set[key] = DateTime.UtcNow;
            _items[key] = value;
        }

        public CacheResult<T> GetOrSet(string key, T value)
        {
            var success = _items.TryGetValue(key, out var item);
            _lastAccess[key] = DateTime.UtcNow;
            if (success)
                return new (CacheResult.CacheHit, item);
            _set[key] = DateTime.UtcNow;
            _items[key] = value;
            return new (CacheResult.CacheMiss, value);
        }

        public T Remove(string key)
        {
            _lastAccess.TryRemove(key, out _);
            _set.TryRemove(key, out _);
            _items.TryRemove(key, out var value);
            return value;
        }

        private void ExpireItems()
        {
            try
            {
                foreach (var keyValuePair in _lastAccess)
                {
                    if (keyValuePair.Value > DateTime.UtcNow.AddMilliseconds(-this.TimeoutInMs)
                        && _set[keyValuePair.Key] > DateTime.UtcNow.AddMilliseconds(-this.MaxAgeInMs))
                        continue;

                    Remove(keyValuePair.Key);
                }
            } 
            catch (Exception e)
            {
                Console.Error.WriteLineAsync(e.ToString());
                // If we can't expire cleanly, we'll need to wipe to ensure latest data rather than force cache
                this.Clear(); 
            }
        }

        public void Clear()
        {
            this._set.Clear();
            this._lastAccess.Clear();
            this._items.Clear();
        }

        public void Dispose()
        {
            if (!_disposed)
                _timer.Dispose();

            _disposed = true;
        }
    }
}
