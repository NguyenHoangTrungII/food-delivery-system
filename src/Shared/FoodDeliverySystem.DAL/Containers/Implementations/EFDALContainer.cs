using FoodDeliverySystem.Common.Logging;
using FoodDeliverySystem.DataAccess.Configurations;
using FoodDeliverySystem.DataAccess.Containers.Interfaces;
using FoodDeliverySystem.DataAccess.Contexts.Interfaces;
using FoodDeliverySystem.DataAccess.Repositories.Implementations;
//using FoodDeliverySystem.DataAccess.UnitOfWork.Interfaces;
//using FoodDeliverySystem.DataAccess.UnitOfWork.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using FoodDeliverySystem.DataAccess.UoW.Implementations;
using FoodDeliverySystem.DataAccess.UoW.Interfaces;

namespace FoodDeliverySystem.DataAccess.Containers.Implementations;

public class EFDALContainer<TDbContext> : IEFDALContainer
    where TDbContext : DbContext, IDbContext
{
    public IDbContext DbContext { get; }
    public IUnitOfWork UnitOfWork { get; }
    public DatabaseConfig DatabaseConfig { get; }

    public EFDALContainer(
        IConfiguration configuration,
        TDbContext dbContext,
        ILoggerAdapter<GenericRepository> logger)
    {
        DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        // Lấy cấu hình database
        var sectionName = typeof(TDbContext).Name.Replace("DbContext", "");
        DatabaseConfig = configuration.GetSection($"Database:Services:{sectionName}").Get<DatabaseConfig>()
            ?? throw new InvalidOperationException($"Database configuration for {sectionName} not found.");

        // Khởi tạo GenericRepository và UnitOfWork
        var genericRepository = new GenericRepository(DbContext, logger);
        UnitOfWork = new UnitOfWork(DbContext, genericRepository);
    }
}