using CatFact.Applications.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace CatFact.Infrastructures.Services
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache? _cache;
        public CacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public T GetCachedData<T>(string key)
        {
            var jsonData = _cache.GetString(key);
            if (jsonData == null)
                return default(T);
            return JsonSerializer.Deserialize<T>(jsonData);
        }

        public void SetCachedData<T>(string key, T data, TimeSpan cacheDuration)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = cacheDuration
            };
            var jsonData = JsonSerializer.Serialize(data);
            _cache.SetString(key, jsonData, options);
        }
    }
}
