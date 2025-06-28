using FoodDeliverySystem.Common.Weather.Configurations;
using FoodDeliverySystem.Common.Weather.Implementations;
using FoodDeliverySystem.Common.Weather.Interfaces;
using FoodDeliverySystem.Common.Weather.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FoodDeliverySystem.Common.Weather.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWeatherService(this IServiceCollection services, string configSection = "Weather:OpenWeather")
    {
        services.AddHttpClient();
        services.AddMemoryCache();
        services.AddScoped<IWeatherService, OpenWeatherService>();
        services.AddScoped<OpenWeatherConfig>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var config = new OpenWeatherConfig();
            configuration.GetSection(configSection).Bind(config);
            return config;
        });
        return services;
    }
}