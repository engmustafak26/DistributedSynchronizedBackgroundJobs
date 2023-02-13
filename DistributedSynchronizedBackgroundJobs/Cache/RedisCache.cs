using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text.Json;

namespace DistributedSynchronizedBackgroundJobs.Cache
{
    public class RedisCache : ICache
    {
        private readonly IDistributedCache _writeCache;
        private readonly IDistributedCache _readCache;
        public RedisCache(IDistributedCache writeCache, IDistributedCache readCache)
        {
            _writeCache = writeCache;
            _readCache = readCache;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var jsonData = await _readCache.GetStringAsync(key);
            return string.IsNullOrWhiteSpace(jsonData) ? default : JsonSerializer.Deserialize<T>(jsonData, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }



        public async Task SetAsync<T>(string key, T data, TimeSpan? absoluteExpireTime = null, TimeSpan? unUsedExpireTime = null)
        {

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpireTime,
                SlidingExpiration = unUsedExpireTime
            };

            var jsonData = JsonSerializer.Serialize(data);
            await _writeCache.SetStringAsync(key, jsonData, options);
        }

        public async Task SetAsync(string key, string jsonData, TimeSpan? absoluteExpireTime = null, TimeSpan? unUsedExpireTime = null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpireTime,
                SlidingExpiration = unUsedExpireTime
            };

            await _writeCache.SetStringAsync(key, jsonData, options);
        }
    }
}
