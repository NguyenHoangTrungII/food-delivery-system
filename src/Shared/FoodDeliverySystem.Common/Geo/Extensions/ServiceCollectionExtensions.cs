using FoodDeliverySystem.Common.Caching.Services;
using FoodDeliverySystem.Common.Geo.Configurations;
using FoodDeliverySystem.Common.Geo.Implementations;
using FoodDeliverySystem.Common.Geo.Implementations.FeeCalculators;
using FoodDeliverySystem.Common.Geo.Interfaces;
using FoodDeliverySystem.Common.Geo.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using FoodDeliverySystem.Common.Geo.Implementations.DistanceCalculators;
using FoodDeliverySystem.Common.Geo.Implementations.MapApiClients;
using FoodDeliverySystem.Common.Geo.Implementations.ETACalculators;
using FoodDeliverySystem.Common.Weather.Extensions;

namespace FoodDeliverySystem.Common.Geo.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGeoDistance(
        this IServiceCollection services,
        Action<GeoOptions> configureOptions)
    {
        // Đăng ký cấu hình GeoOptions
        services.Configure<GeoOptions>(configureOptions);

        // Đăng ký HttpClient cho OSRM
        services.AddHttpClient<IMapApiClient, OsrmApiClient>(client =>
        {
            var geoOptions = services.BuildServiceProvider().GetRequiredService<IOptions<GeoOptions>>().Value;
            client.BaseAddress = new Uri(geoOptions.OSRM?.Url ?? "http://localhost:5000");
        });

        // Đăng ký IGeoDistanceCalculator với dependency injection dựa trên cấu hình
        services.AddScoped<IGeoDistanceCalculator>(sp =>
        {
            var geoOptions = sp.GetRequiredService<IOptions<GeoOptions>>().Value;
            var innerCalculator = GetInnerCalculator(sp, geoOptions.Engine.Engine);

            return new CachedGeoDistanceCalculator(
                sp.GetRequiredService<ICacheService>(),
                innerCalculator,
                sp.GetRequiredService<IMapApiClient>(),
                sp.GetRequiredService<ILogger<CachedGeoDistanceCalculator>>(),
                geoOptions.Redis?.GeoDistanceTTL ?? TimeSpan.FromMinutes(30));
        });

        // Đăng ký các calculator cơ bản
        services.AddScoped<HaversineGeoDistanceCalculator>();
        services.AddScoped<PostgisGeoDistanceCalculator>();
        services.AddScoped<OsrmGeoDistanceCalculator>();
        services.AddScoped<MongoGeoDistanceCalculator<IGeoEntity>>();

        // Đăng ký IGeoDatabaseProvider với cấu hình từ GeoOptions
        services.AddScoped<IGeoDatabaseProvider>(sp =>
        {
            var geoOptions = sp.GetRequiredService<IOptions<GeoOptions>>().Value;
            return new PostgisGeoDatabaseProvider(geoOptions.PostGIS?.ConnectionString);
        });

        // Đăng ký MongoDB với cấu hình từ GeoOptions
        services.AddSingleton<IMongoDatabase>(sp =>
        {
            var geoOptions = sp.GetRequiredService<IOptions<GeoOptions>>().Value;
            var client = new MongoClient(geoOptions.Mongo?.ConnectionString);
            return client.GetDatabase(geoOptions.Mongo?.DatabaseName);
        });

        services.AddSingleton<IGeoDistanceCalculatorFactory>(sp => new GeoDistanceCalculatorFactory(sp));

        services.AddScoped<IDeliveryFeeCalculator, DynamicFeeCalculator>();
        services.AddScoped<IETACalculator, DynamicETACalculator>();
        services.AddHttpClient<IGeocodingService, NominatimGeocodingService>(client =>
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("FoodDeliverySystem/1.0 (https://localhost)");
        });
        services.AddWeatherService();
        //services.AddHttpClient<WeatherService>();
        //services.AddScoped<DriverAllocationService>();

        services.AddWeatherService();


        return services;
    }

    private static IGeoDistanceCalculator GetInnerCalculator(IServiceProvider sp, string engine)
    {
        return engine switch
        {
            "OSRM" => sp.GetRequiredService<OsrmGeoDistanceCalculator>(),
            "PostGIS" => sp.GetRequiredService<PostgisGeoDistanceCalculator>(),
            "Mongo" => sp.GetServices<IGeoDistanceCalculator>()
                .FirstOrDefault(c => c.GetType().IsGenericType && c.GetType().GetGenericTypeDefinition() == typeof(MongoGeoDistanceCalculator<>))
                ?? throw new InvalidOperationException("MongoGeoDistanceCalculator<T> not registered."),
            _ => sp.GetRequiredService<HaversineGeoDistanceCalculator>()
        };
    }

    public interface IGeoDistanceCalculatorFactory
    {
        IGeoDistanceCalculator GetCalculator(string engine);
    }

    public class GeoDistanceCalculatorFactory : IGeoDistanceCalculatorFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public GeoDistanceCalculatorFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IGeoDistanceCalculator GetCalculator(string engine)
        {
            return engine switch
            {
                "PostGIS" => _serviceProvider.GetRequiredService<PostgisGeoDistanceCalculator>(),
                "OSRM" => _serviceProvider.GetRequiredService<OsrmGeoDistanceCalculator>(),
                "Mongo" => _serviceProvider.GetServices<IGeoDistanceCalculator>()
                    .FirstOrDefault(c => c.GetType().IsGenericType && c.GetType().GetGenericTypeDefinition() == typeof(MongoGeoDistanceCalculator<>))
                    ?? throw new InvalidOperationException("MongoGeoDistanceCalculator<T> not registered."),
                _ => _serviceProvider.GetRequiredService<HaversineGeoDistanceCalculator>()
            };
        }
    }

}

