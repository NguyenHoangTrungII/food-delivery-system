using System.Linq.Expressions;

namespace FoodDeliverySystem.DataAccess.Repositories.Interfaces;

public interface IGenericRepository
{
    Task AddAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class;
    Task AddRangeAsync<T>(IEnumerable<T> entities, CancellationToken cancellationToken = default) where T : class;
    Task UpdateAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class;
    Task DeleteAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) where T : class;
    Task<T?> GetByIdAsync<T>(Guid id, CancellationToken cancellationToken = default) where T : class;
    IQueryable<T> GetAsync<T>(Expression<Func<T, bool>> predicate) where T : class;
    Task<T?> FirstOrDefaultAsync<T>(
        Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IQueryable<T>>? include = null,
        CancellationToken cancellationToken = default) where T : class;
}