using FoodDeliverySystem.Common.Caching.Dependencies;
using FoodDeliverySystem.Common.Caching.Models;
using StackExchange.Redis;
using System.Text.Json;
using Sustainsys.Saml2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoodDeliverySystem.Common.Caching.Configuration;
using Microsoft.Extensions.Options;

namespace FoodDeliverySystem.Common.Caching.Services;

public class RedisCacheService : ICacheService
{

    private readonly RedisConnectionFactory _redis;

    public RedisCacheService(IOptions<CacheConfig> options)
    {
        var connectionString = options.Value.RedisConnectionString
            ?? throw new ArgumentNullException("Redis ConnectionString is missing in config.");

        _redis = new RedisConnectionFactory(connectionString);
    }

    //private readonly RedisConnectionFactory _redis;

    //public RedisCacheService(string connectionString)
    //{
    //    _redis = new RedisConnectionFactory(connectionString);
    //}

    //private readonly IConnectionMultiplexer _redis;
    //private readonly ILoggerAdapter<RedisCacheService> _logger;

    //public RedisCacheService(CacheConfig config)
    //{
    //    _redis = ConnectionMultiplexer.Connect(config.RedisConnectionString);
    //    //_logger = logger;
    //}

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var value = await db.StringGetAsync(key);
        if (value.IsNullOrEmpty)
        {
            return default;
        }
        return JsonSerializer.Deserialize<T>(value);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var serializedValue = JsonSerializer.Serialize(value);
        await db.StringSetAsync(key, serializedValue, ttl);
    }

    public async Task<bool> HashSetAsync(string key, string field, bool value, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var result = await db.HashSetAsync(key, field, value);
        await db.KeyExpireAsync(key, ttl);
        return result;
    }

    public async Task<bool> HashSetAsync(
    string key,
    Dictionary<string, bool> values,
    TimeSpan expiry,
    CancellationToken cancellationToken = default)
    {
        if (values == null || !values.Any())
            return false;

        var entries = values
            .Select(kv => new HashEntry(kv.Key, kv.Value))
            .ToArray();

        var db = _redis.GetDatabase();

        await db.HashSetAsync(key, entries);

        // Đặt TTL
        await db.KeyExpireAsync(key, expiry);

        return true;
    }


    public async Task<bool?> HashGetAsync(string key, string field, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var value = await db.HashGetAsync(key, field);
        if (value.IsNullOrEmpty)
        {
            return null;
        }
        return (bool)value;
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync(key);
    }

    public async Task UpdateTTLAsync(string key, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        await db.KeyExpireAsync(key, ttl);
    }
}
