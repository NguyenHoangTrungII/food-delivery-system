using FoodDeliverySystem.DataAccess.Configurations;
using FoodDeliverySystem.DataAccess.Contexts.Interfaces;
using FoodDeliverySystem.DataAccess.UoW.Interfaces;

namespace FoodDeliverySystem.DataAccess.Containers.Interfaces;

public interface IEFDALContainer
{

    IDbContext DbContext { get; }
    IUnitOfWork UnitOfWork { get; }
    DatabaseConfig DatabaseConfig { get; }

}