using FoodDeliverySystem.DataAccess.Configurations;
using FoodDeliverySystem.DataAccess.Containers.Implementations;
using FoodDeliverySystem.DataAccess.Containers.Interfaces;
using FoodDeliverySystem.DataAccess.Contexts.Interfaces;
using FoodDeliverySystem.DataAccess.Providers.Implementations;
using FoodDeliverySystem.DataAccess.Providers.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
//using MySql.EntityFrameworkCore.Extensions;

namespace FoodDeliverySystem.DataAccess.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection UseDAL<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        string configSection = "Database:Services")
        where TDbContext : DbContext, IDbContext
    {
        // Lấy cấu hình database
        var sectionName = typeof(TDbContext).Name.Replace("DbContext", "");
        var dbConfig = configuration.GetSection($"{configSection}:{sectionName}").Get<DatabaseConfig>()
            ?? throw new InvalidOperationException($"Database configuration for {sectionName} not found.");

        // Đăng ký DbContext
        services.AddDbContext<TDbContext>(options =>
        {
            switch (dbConfig.DatabaseType.ToLower())
            {
                //case "mysql":
                //    options.UseMySql(dbConfig.ConnectionString, ServerVersion.AutoDetect(dbConfig.ConnectionString),
                //        opt => opt.EnableRetryOnFailure(dbConfig.MaxRetryAttempts));
                //    break;
                case "postgresql":
                    options.UseNpgsql(dbConfig.ConnectionString,
                        opt => opt.EnableRetryOnFailure(dbConfig.MaxRetryAttempts));
                    break;
                case "sqlserver":
                    options.UseSqlServer(dbConfig.ConnectionString,
                        opt => opt.EnableRetryOnFailure(dbConfig.MaxRetryAttempts));
                    break;
                default:
                    throw new NotSupportedException($"Database type {dbConfig.DatabaseType} is not supported.");
            }
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        // Đăng ký EFDALContainer và các thành phần khác
        services.AddScoped<IEFDALContainer, EFDALContainer<TDbContext>>();
        services.AddScoped<IDbContext>(sp => sp.GetRequiredService<TDbContext>());
        services.AddSingleton<IDbContextFactory, DynamicDbContextFactory>();

        return services;
    }
}