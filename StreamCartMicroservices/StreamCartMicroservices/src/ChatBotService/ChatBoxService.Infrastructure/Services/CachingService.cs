using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace ChatBoxService.Infrastructure.Services
{
    public class CachingService : ICachingService
    {
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(30);

        public CachingService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public T Get<T>(string key) where T : class
        {
            _cache.TryGetValue(key, out T value);
            return value;
        }

        public void Set<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? _defaultExpiration
            };
            _cache.Set(key, value, cacheOptions);
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class
        {
            if (_cache.TryGetValue(key, out T cachedValue))
                return cachedValue;

            var value = await factory();

            if (value != null)
            {
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration ?? _defaultExpiration
                };
                _cache.Set(key, value, cacheOptions);
            }

            return value;
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }
    }

    public interface ICachingService
    {
        T Get<T>(string key) where T : class;
        void Set<T>(string key, T value, TimeSpan? expiration = null) where T : class;
        Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class;
        void Remove(string key);
    }
}