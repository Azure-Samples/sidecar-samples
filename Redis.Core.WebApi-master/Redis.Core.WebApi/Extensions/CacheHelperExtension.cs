using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Redis.Core.WebApi.Extensions
{
    public static class CacheHelperExtension
    {
        public static async Task SetRecordAsync<T>(this IDistributedCache cache,
            T value,
            string key,
            TimeSpan? absoluteExpiry=null,
            TimeSpan? slidingExpiry=null)
        {
            //Caching option
            var cachingOption = new DistributedCacheEntryOptions();

            //Cache Time to Live, from Current Time to <specified> mins, after that cache will expire
            cachingOption.AbsoluteExpirationRelativeToNow = absoluteExpiry ?? TimeSpan.FromSeconds(60);

            //Cache Time to Live , will slide if it is accessed withing the sliding window
            cachingOption.SlidingExpiration = slidingExpiry;

            //Convert to JSON
            var dataToStore = JsonSerializer.Serialize(value);

            //Set Cache
            await cache.SetStringAsync(key, dataToStore, cachingOption);
        }

        public static async Task<T> GetRecordAsync<T>(this IDistributedCache cache, string key)
        {
            //Read Data from Cache
            var jsonData = await cache.GetStringAsync(key);

            //Cache not available, return default of type T
            if (jsonData == null)
                return default(T);

            //Deserialize the Data
            return JsonSerializer.Deserialize<T>(jsonData);
        }
    }
}
