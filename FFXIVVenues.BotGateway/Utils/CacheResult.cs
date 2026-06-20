namespace FFXIVVenues.Veni.Utils;

public enum CacheResult
{
    CacheHit, 
    CacheMiss
}

public record CacheResult<T>(CacheResult Result, T Value);