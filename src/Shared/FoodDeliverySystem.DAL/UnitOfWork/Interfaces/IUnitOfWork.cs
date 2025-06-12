using FoodDeliverySystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace FoodDeliverySystem.DataAccess.UoW.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository Repository { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitAsync(IDbContextTransaction transaction, CancellationToken cancellationToken = default);
    Task RollbackAsync(IDbContextTransaction transaction, CancellationToken cancellationToken = default);
    //DatabaseFacade Database { get; }
}