//using EffiAP.Domain.SeedWork;
//using Microsoft.EntityFrameworkCore.Query;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Text;
//using System.Threading.Tasks;

//namespace AuthService.Infrastructure.IRepositories
//{
//    public interface IGenericRepository : IScopedService
//    {
//        IQueryable<T> GetAll<T>() where T : class;
//        IQueryable<T> GetAllAsync<T>() where T : class;

//        Task AddAsync<T>(T entity) where T : class;

//        Task AddRangeAsync<T>(IEnumerable<T> entities) where T : class; 
//        Task UpdateAsync<T>(T entity) where T : class;
//        Task DeleteAsync<T>(Expression<Func<T, bool>> predicate) where T : class;
//        //Task SaveChangesAsync();

//        Task<T> GetOneAsync<T>(Expression<Func<T, bool>> predicate) where T : class;
//        IQueryable<T> GetAsync<T>(Expression<Func<T, bool>> predicate) where T : class;

//        IQueryable<T> Get<T>(Expression<Func<T, bool>> predicate = null, params Expression<Func<T, object>>[] includes) where T : class;
//        IQueryable<T> Get<T>(Expression<Func<T, bool>> predicate) where T : class;

//        Task<bool> ExistsAsync<T>(int id) where T : class;
//        //Task<bool> SaveAsync();

//        Task<T> GetByIdAsync<T>(Guid id) where T : class;


//        Task<T> GetByIdAsync<T>(object id) where T : class;
//        Task<IEnumerable<T>> FindAsync<T>(Expression<Func<T, bool>> predicate) where T : class;
//        //Task<T> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> predicate) where T : class;

//        //Task<T> FirstOrDefaultAsync<T>(
//        //   Expression<Func<T, bool>> predicate,
//        //   Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null) where T : class;

//        Task<T?> FirstOrDefaultAsync<T>(
//        Expression<Func<T, bool>> predicate,
//        Func<IQueryable<T>, IQueryable<T>>? include = null,
//        CancellationToken cancellationToken = default)
//        where T : class;



//        void Add<T>(T entity) where T : class;
//        void Update<T>(T entity) where T : class;
//        void Remove<T>(T entity) where T : class;
//        void RemoveRange<T>(IEnumerable<T> entities) where T : class;




//    }
//}
