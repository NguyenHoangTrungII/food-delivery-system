using FoodDeliverySystem.Common.Caching.Configuration;
using FoodDeliverySystem.Common.Geo.Interfaces;
using FoodDeliverySystem.Common.Geo.Implementations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FoodDeliverySystem.Common.Caching.Services;
using Microsoft.Extensions.Options;

namespace FoodDeliverySystem.Common.Geo.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGeoDistance(
        this IServiceCollection services,
        IConfiguration configuration,
        string engine,
        string mongoCollectionName = null,
        string postgisTableName = "locations")
    {
        // Cấu hình CacheOptions
        services.Configure<CacheOptions>(configuration.GetSection("Redis"));

        services.AddScoped<IGeoDistanceCalculator>(sp =>
        {
            var innerCalculator = engine switch
            {
                "PostGIS" => sp.GetRequiredService<PostgisGeoDistanceCalculator>(),
                "Mongo" => sp.GetServices<IGeoDistanceCalculator>()
                    .FirstOrDefault(c => c.GetType().IsGenericType && c.GetType().GetGenericTypeDefinition() == typeof(MongoGeoDistanceCalculator<>))
                    ?? throw new InvalidOperationException("MongoGeoDistanceCalculator<T> not registered for Mongo engine. Register it in your microservice."),
                _ => sp.GetRequiredService<HaversineGeoDistanceCalculator>()
            };

            var cacheOptions = sp.GetRequiredService<IOptions<CacheOptions>>().Value;
            var cacheDuration = cacheOptions.DefaultTTL; // Hoặc thêm GeoDistanceTTL trong CacheOptions

            return new CachedGeoDistanceCalculator(
                sp.GetRequiredService<ICacheService>(),
                innerCalculator,
                sp.GetRequiredService<ILogger<CachedGeoDistanceCalculator>>(),
                cacheDuration);
        });

        services.AddScoped<HaversineGeoDistanceCalculator>();
        services.AddScoped<PostgisGeoDistanceCalculator>(sp =>
            new PostgisGeoDistanceCalculator(
                sp.GetRequiredService<IGeoDatabaseProvider>(),
                sp.GetRequiredService<ILogger<PostgisGeoDistanceCalculator>>(),
                postgisTableName));

        return services;
    }
}