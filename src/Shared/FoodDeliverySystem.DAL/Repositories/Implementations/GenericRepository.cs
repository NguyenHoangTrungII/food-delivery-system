using FoodDeliverySystem.DataAccess.Contexts.Interfaces;
using FoodDeliverySystem.DataAccess.Entities;
using FoodDeliverySystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using FoodDeliverySystem.Common.Logging;

namespace FoodDeliverySystem.DataAccess.Repositories.Implementations;

public class GenericRepository : IGenericRepository
{
    private readonly IDbContext _dbContext;
    private readonly ILoggerAdapter<GenericRepository> _logger;

    public GenericRepository(IDbContext dbContext, ILoggerAdapter<GenericRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task AddAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        _logger.LogInformation("Adding entity of type {EntityType}", typeof(T).Name);
        if (entity is BaseEntity baseEntity)
        {
            baseEntity.CreatedAt = DateTime.UtcNow;
            baseEntity.Id = baseEntity.Id == Guid.Empty ? Guid.NewGuid() : baseEntity.Id;
        }
        await _dbContext.Set<T>().AddAsync(entity, cancellationToken);
    }

    public async Task AddRangeAsync<T>(IEnumerable<T> entities, CancellationToken cancellationToken = default) where T : class
    {
        if (entities == null || !entities.Any()) throw new ArgumentNullException(nameof(entities));
        _logger.LogInformation("Adding {Count} entities of type {EntityType}", entities.Count(), typeof(T).Name);
        foreach (var entity in entities)
        {
            if (entity is BaseEntity baseEntity)
            {
                baseEntity.CreatedAt = DateTime.UtcNow;
                baseEntity.Id = baseEntity.Id == Guid.Empty ? Guid.NewGuid() : baseEntity.Id;
            }
        }
        await _dbContext.Set<T>().AddRangeAsync(entities, cancellationToken);
    }

    public async Task UpdateAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        _logger.LogInformation("Updating entity of type {EntityType}", typeof(T).Name);
        if (entity is BaseEntity baseEntity)
        {
            baseEntity.ModifiedAt = DateTime.UtcNow;
        }
        _dbContext.Set<T>().Update(entity);
    }

    public async Task DeleteAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) where T : class
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        _logger.LogInformation("Deleting entities of type {EntityType}", typeof(T).Name);
        var entities = await _dbContext.Set<T>().Where(predicate).ToListAsync(cancellationToken);
        foreach (var entity in entities)
        {
            if (entity is BaseEntity baseEntity)
            {
                //baseEntity.IsDeleted = true;
                baseEntity.ModifiedAt = DateTime.UtcNow;
                _dbContext.Set<T>().Update(entity);
            }
            else
            {
                _dbContext.Set<T>().Remove(entity);
            }
        }
    }

    public async Task<T?> GetByIdAsync<T>(Guid id, CancellationToken cancellationToken = default) where T : class
    {
        _logger.LogInformation("Fetching entity of type {EntityType} with ID {Id}", typeof(T).Name, id);
        return await _dbContext.Set<T>().FindAsync(new object[] { id }, cancellationToken);
    }

    public IQueryable<T> GetAsync<T>(Expression<Func<T, bool>> predicate) where T : class
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        _logger.LogInformation("Querying entities of type {EntityType}", typeof(T).Name);
        return _dbContext.Set<T>().Where(predicate);
    }

    public async Task<T?> FirstOrDefaultAsync<T>(
        Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IQueryable<T>>? include = null,
        CancellationToken cancellationToken = default) where T : class
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        _logger.LogInformation("Fetching first or default entity of type {EntityType}", typeof(T).Name);
        var query = _dbContext.Set<T>().AsQueryable();
        if (include != null)
        {
            query = include(query);
        }
        return await query.FirstOrDefaultAsync(predicate, cancellationToken);
    }
}