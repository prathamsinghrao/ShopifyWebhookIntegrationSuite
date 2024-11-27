using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using System.Globalization;

namespace ShopifyWebhookManager.CacheHelper
{
    public class MemoryCacheClient : IMemoryCacheClient
    {
        private readonly IMemoryCache _cache;
        private ConcurrentDictionary<string, DateTime> _cacheKeys;

        public MemoryCacheClient(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
            _cacheKeys = new ConcurrentDictionary<string, DateTime>();
            _cacheKeys.AddOrUpdate("cache_instance", DateTime.Now, (key, oldValue) => DateTime.Now);
        }
        public T Get<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (!_cache.TryGetValue<T>(key.ToLower(CultureInfo.InvariantCulture), out T obj))
            {
                return default(T);
            }
            return obj;
        }
        public void SetWithAbsoluteExpiration<T>(string key, T @object, int seconds)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (@object != null)
            {
                // Set cache options.
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(seconds));

                // Save data in cache.
                _cache.Set(key.ToLower(CultureInfo.InvariantCulture), @object, cacheEntryOptions);
                if (_cacheKeys.ContainsKey(key.ToLower(CultureInfo.InvariantCulture)))
                    _cacheKeys[key.ToLower(CultureInfo.InvariantCulture)] = DateTime.Now;
                else
                    _cacheKeys.AddOrUpdate(key.ToLower(CultureInfo.InvariantCulture), DateTime.Now, (key, oldValue) => DateTime.Now);
            }
        }
    }
}
