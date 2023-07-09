using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Auction.API {
    public static class DistributedCacheExtensions {
        public static async Task SetRecordAsync<T>(
            this IDistributedCache cache, 
            string recordId, 
            T data, 
            TimeSpan? absoluteExpireTime = null, 
            TimeSpan? unusedExpireTime = null
        ) {
            var options = new DistributedCacheEntryOptions();
            options.AbsoluteExpirationRelativeToNow = absoluteExpireTime ?? TimeSpan.FromSeconds(60);
            options.SlidingExpiration = unusedExpireTime;

            var jsonData = JsonSerializer.Serialize(data);
            await cache.SetStringAsync(recordId, jsonData, options);
        }

        public static async Task<T?> GetRecordAsync<T>(
            this IDistributedCache cache,
            string recordId
        ) {
            var jsonData = await cache.GetStringAsync(recordId);
            if(jsonData is null) {
                return default(T);
            } else {
                return JsonSerializer.Deserialize<T>(jsonData);
            }
        }

        public static T[] AddAndReturn<T>(this T[] array, T obj) {
            var newArray = new T[array.Length+1];
            for(int i = 0; i < array.Length; i++) {
                newArray[i] = array[i]; 
            }
            newArray[array.Length] = obj;
            return newArray;
        }
    }
}
