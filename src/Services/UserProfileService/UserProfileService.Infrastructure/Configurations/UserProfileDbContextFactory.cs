using FoodDeliverySystem.DataAccess.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using UserProfileService.Infrastructure.Configurations;

namespace UserProfileService.Infrastructure.Configurations;

public class UserProfileDbContextFactory : IDesignTimeDbContextFactory<UserProfileDbContext>
{
    public UserProfileDbContext CreateDbContext(string[] args)
    {
        // Load cấu hình từ appsettings.json
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .Build();

        // Lấy cấu hình database
        var sectionName = nameof(UserProfileDbContext).Replace("DbContext", "");
        var dbConfig = configuration.GetSection($"Database:Services:{sectionName}").Get<DatabaseConfig>()
            ?? throw new InvalidOperationException($"Database configuration for {sectionName} not found.");

        // Log để debug
        Console.WriteLine($"DatabaseConfig: Type={dbConfig.DatabaseType}, ConnectionString={dbConfig.ConnectionString}");

        // Tạo DbContextOptions
        var optionsBuilder = new DbContextOptionsBuilder<UserProfileDbContext>();
        switch (dbConfig.DatabaseType.ToLower())
        {
            //case "mysql":
            //    optionsBuilder.UseMySql(dbConfig.ConnectionString, ServerVersion.AutoDetect(dbConfig.ConnectionString),
            //        opt => opt.EnableRetryOnFailure(dbConfig.MaxRetryAttempts));
            //    break;
            case "postgresql":
                optionsBuilder.UseNpgsql(dbConfig.ConnectionString,
                    opt => opt.EnableRetryOnFailure(dbConfig.MaxRetryAttempts));
                break;
            case "sqlserver":
                optionsBuilder.UseSqlServer(dbConfig.ConnectionString,
                    opt => opt.EnableRetryOnFailure(dbConfig.MaxRetryAttempts));
                break;
            default:
                throw new NotSupportedException($"Database type {dbConfig.DatabaseType} is not supported.");
        }

        return new UserProfileDbContext(optionsBuilder.Options);
    }
}