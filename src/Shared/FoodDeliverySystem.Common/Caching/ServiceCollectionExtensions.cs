using FoodDeliverySystem.Common.Caching.Configuration;
using FoodDeliverySystem.Common.Caching.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FoodDeliverySystem.Common.Caching;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRedisCaching(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind configuration vào CacheOptions
        services.Configure<CacheOptions>(configuration.GetSection("Redis"));

        // Đăng ký RedisCacheService
        services.AddSingleton<ICacheService, RedisCacheService>();

        return services;
    }
}


//using FoodDeliverySystem.Common.Caching.Services;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;

//namespace FoodDeliverySystem.Common.Caching;

//public static class ServiceCollectionExtensions
//{
//    public static IServiceCollection AddRedisCaching(this IServiceCollection services, IConfiguration configuration)
//    {
//        var connectionString = configuration["Redis:ConnectionString"]
//            ?? throw new ArgumentNullException("Redis:ConnectionString is missing in configuration.");

//        services.AddSingleton<ICacheService>(sp => new RedisCacheService(connectionString));
//        return services;
//    }
//}