using FoodDeliverySystem.DataAccess.Contexts.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FoodDeliverySystem.DataAccess.Providers.Interfaces;

public interface IDbContextFactory
{
    TDbContext CreateDbContext<TDbContext>(
        string databaseType,
        string connectionString,
        Action<DbContextOptionsBuilder>? configure = null)
        where TDbContext : DbContext, IDbContext;
}