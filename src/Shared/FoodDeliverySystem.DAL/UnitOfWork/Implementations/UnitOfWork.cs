using FoodDeliverySystem.DataAccess.Contexts.Interfaces;
using FoodDeliverySystem.DataAccess.Repositories.Interfaces;
using FoodDeliverySystem.DataAccess.UoW.Interfaces;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace FoodDeliverySystem.DataAccess.UoW.Implementations;

public class UnitOfWork : IUnitOfWork
{
    private readonly IDbContext _dbContext;
    private readonly IGenericRepository _repository;

    public UnitOfWork(IDbContext dbContext, IGenericRepository repository)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public IGenericRepository Repository => _repository;
    public DatabaseFacade DatabaseContext => _dbContext.Database;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitAsync(IDbContextTransaction transaction, CancellationToken cancellationToken = default)
    {
        if (transaction == null) throw new ArgumentNullException(nameof(transaction));
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task RollbackAsync(IDbContextTransaction transaction, CancellationToken cancellationToken = default)
    {
        if (transaction == null) throw new ArgumentNullException(nameof(transaction));
        await transaction.RollbackAsync(cancellationToken);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}