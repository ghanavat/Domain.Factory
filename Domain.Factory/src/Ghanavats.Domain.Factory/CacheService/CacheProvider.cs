using Ghanavats.Domain.Factory.Abstractions;
using Ghanavats.Domain.Primitives.Extensions;
using Microsoft.Extensions.Caching.Memory;

namespace Ghanavats.Domain.Factory.CacheService;

public sealed class CacheProvider : ICacheProvider
{
    private readonly IMemoryCache _memoryCache;
    
    public CacheProvider(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache.CheckForNull();
    }

    public object Get(object key)
    {
        if (key.ToString() == string.Empty) return string.Empty;

        var getValueResult = _memoryCache.TryGetValue(key, out var value);

        if (value is null)
        {
            return string.Empty;
        }
        
        return getValueResult ? value : string.Empty;
    }

    public object Insert(object key, object value)
    {
        if (key.ToString() == string.Empty 
            || value.ToString() == string.Empty)
        {
            return string.Empty;
        }

        return _memoryCache.Set(key, value);
    }

    public object Insert(object key, object value, TimeSpan timeTillExpires)
    {
        throw new NotImplementedException();
    }

    public object Insert(object key, object value, DateTime absoluteExpiration)
    {
        throw new NotImplementedException();
    }

    public object Update(object key, object value)
    {
        throw new NotImplementedException();
    }

    public object Update(object key, object value, TimeSpan timeTillExpires)
    {
        throw new NotImplementedException();
    }

    public object Update(object key, object value, DateTime absoluteExpiration)
    {
        throw new NotImplementedException();
    }

    public object Remove(object key)
    {
        throw new NotImplementedException();
    }
}
