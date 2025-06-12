// src/common/DependencyInjection.cs
using FoodDeliverySystem.Common.Authorization;
using FoodDeliverySystem.Common.Authorization.Implementations;
using FoodDeliverySystem.Common.Authorization.Interfaces;
using FoodDeliverySystem.Common.Caching;
using FoodDeliverySystem.Common.Caching.Configuration;
using FoodDeliverySystem.Common.Caching.Services;
using FoodDeliverySystem.Common.Email.Configuration;
using FoodDeliverySystem.Common.Email.Interfaces;
using FoodDeliverySystem.Common.Email.Services;
using FoodDeliverySystem.Common.Logging;
using FoodDeliverySystem.Common.Messaging;
using FoodDeliverySystem.Common.Messaging.Configuration;
using FoodDeliverySystem.Common.Messaging.Implementations;
using FoodDeliverySystem.Common.Messaging.Interfaces;
using FoodDeliverySystem.Common.Messaging.Models;
using FoodDeliverySystem.Common.ServiceClient.Interfaces;
using FoodDeliverySystem.Common.ServiceClient.Implementations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using FoodDeliverySystem.Common.ServiceClient.Configuration;
using Polly.Extensions.Http;
using Polly;
using Microsoft.Extensions.Options;


namespace FoodDeliverySystem.Common;

public static class DependencyInjection
{
    public static IServiceCollection AddCommonServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Đăng ký Authorization
        services.AddSingleton<IPermissionChecker, PermissionChecker>();
        services.AddSingleton<IPermissionCacheService, RedisPermissionCacheService>();

        // Đăng ký Caching
        services.AddSingleton<ICacheService, RedisCacheService>();
        services.AddSingleton<CacheOptions>();

        // Đăng ký Logging
        services.AddSingleton(typeof(ILoggerAdapter<>), typeof(LoggerAdapter<>));

        // Đăng ký Redis
        //services.AddStackExchangeRedisCache(options =>
        //{
        //    options.Configuration = "localhost:6379"; // Có thể lấy từ configuration
        //});

        services.AddSingleton<IEmailService, SmtpEmailService>();
        services.Configure<EmailConfig>(configuration.GetSection("Email"));

        services.AddRedisCaching(configuration);

        //services.Configure<CacheOptions>(configuration.GetSection("Cache"));
        //services.Configure<MessageQueueConfig>(configuration.GetSection("MessageQueue"));
        //services.AddSingleton<IMessageConsumer, RabbitMQConsumer>();
        //services.AddHostedService<PermissionCacheInvalidationHostedService>();

        services.Configure<MessageQueueConfig>(configuration.GetSection("MessageQueue:PermissionChanges"));
        services.AddSingleton<IMessageHandler<PermissionChangeMessage>, PermissionChangeMessageHandler>();
        services.AddSingleton<IMessageConsumer, RabbitMQConsumer<PermissionChangeMessage>>();
        services.AddHostedService<MessageConsumerHostedService<PermissionChangeMessage>>();

        // Bind cấu hình từ section ApiGateway
        services.Configure<ServiceClientConfig>(configuration.GetSection("ApiGateway"));

        services.Configure<ServiceClientConfig>(configuration.GetSection("ApiGateway"));

        services.AddHttpContextAccessor();



        services.AddHttpClient("ServiceClientBase") // chỉ để tạo HttpClient, không gắn với typed client
            .AddPolicyHandler((provider, _) =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                return HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .WaitAndRetryAsync(
                        retryCount: config.GetValue<int>("ApiGateway:MaxRetryAttempts"),
                        sleepDurationProvider: attempt =>
                            TimeSpan.FromMilliseconds(config.GetValue<int>("ApiGateway:RetryDelayMilliseconds") * attempt));
            });

        services.AddTransient<IServiceClient, FoodDeliverySystem.Common.ServiceClient.Implementations.ServiceClient>();




        return services;
    }
}