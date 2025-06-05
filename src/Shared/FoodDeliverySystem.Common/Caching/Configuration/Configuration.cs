// src/common/Caching/Configuration/CacheConfig.cs

namespace FoodDeliverySystem.Common.Caching.Configuration;
public class CacheConfig
{
    public string RedisConnectionString { get; set; } = "localhost:6379";
}
