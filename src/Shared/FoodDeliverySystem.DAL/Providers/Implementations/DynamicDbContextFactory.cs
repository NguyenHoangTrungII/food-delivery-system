using FoodDeliverySystem.DataAccess.Contexts.Interfaces;
using FoodDeliverySystem.DataAccess.Providers.Interfaces;
using Microsoft.EntityFrameworkCore;
//using MySql.EntityFrameworkCore.Extensions;

namespace FoodDeliverySystem.DataAccess.Providers.Implementations;

public class DynamicDbContextFactory : IDbContextFactory
{
    public TDbContext CreateDbContext<TDbContext>(
        string databaseType,
        string connectionString,
        Action<DbContextOptionsBuilder>? configure = null)
        where TDbContext : DbContext, IDbContext
    {
        var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();

        switch (databaseType.ToLower())
        {
            //case "mysql":
            //    optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
                //break;
            case "postgresql":
                optionsBuilder.UseNpgsql(connectionString);
                break;
            case "sqlserver":
                optionsBuilder.UseSqlServer(connectionString);
                break;
            default:
                throw new NotSupportedException($"Database type {databaseType} is not supported.");
        }

        configure?.Invoke(optionsBuilder);
        return (TDbContext)Activator.CreateInstance(typeof(TDbContext), optionsBuilder.Options)!;
    }
}