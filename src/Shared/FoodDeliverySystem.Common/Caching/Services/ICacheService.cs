using FoodDeliverySystem.Common.Caching.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodDeliverySystem.Common.Caching.Services;

public interface ICacheService
{
    //Task<T?> GetAsync<T>(string key);
    //Task SetAsync<T>(string key, T value, CacheEntryOptions? options = null);
    //Task RemoveAsync(string key);
    //Task<bool> ExistsAsync(string key);
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default);
    Task<bool> HashSetAsync(string key, string field, bool value, TimeSpan ttl, CancellationToken cancellationToken = default);
    Task<bool> HashSetAsync(string key, Dictionary<string, bool> values, TimeSpan expiry, CancellationToken cancellationToken = default);

    Task<bool?> HashGetAsync(string key, string field, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task UpdateTTLAsync(string key, TimeSpan ttl, CancellationToken cancellationToken = default);

}
