using Microsoft.Extensions.Caching.Memory;
using System;

namespace ClientApp.Services
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _defaultExpirationTime = TimeSpan.FromMinutes(5);

        public CacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public T GetOrSet<T>(string key, Func<T> getFunction, TimeSpan? expirationTime = null)
        {
            if (_cache.TryGetValue(key, out T cachedValue))
            {
                return cachedValue;
            }

            var value = getFunction();
            var memoryCacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expirationTime ?? _defaultExpirationTime
            };

            _cache.Set(key, value, memoryCacheEntryOptions);
            return value;
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }
    }
}

