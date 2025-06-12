//using AuthService.Infrastructure.Data;
//using AuthService.Infrastructure.IRepositories;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Query;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Text;
//using System.Threading.Tasks;

//namespace AuthService.Infrastructure.Repositories
//{
//    public class GenericRepository : IGenericRepository
//    {
//        private readonly AuthDbContext _context;

//        public GenericRepository(AuthDbContext context)
//        {
//            _context = context;
//        }

//        public IQueryable<T> Get<T>(Expression<Func<T, bool>> predicate = null, params Expression<Func<T, object>>[] includes) where T : class
//        {
//            IQueryable<T> query = _context.Set<T>();

//            // Include các thực thể liên quan nếu có
//            foreach (var include in includes)
//            {
//                query = query.Include(include);
//            }

//            // Áp dụng bộ lọc nếu có
//            if (predicate != null)
//            {
//                query = query.Where(predicate);
//            }

//            return query;
//        }

//        public IQueryable<T> GetAll<T>() where T : class
//        {
//            return _context.Set<T>();
//        }

//        public async Task AddAsync<T>(T entity) where T : class
//        {
//            await _context.Set<T>().AddAsync(entity);
//        }

//        public async Task AddRangeAsync<T>(IEnumerable<T> entities) where T : class
//        {
//            await _context.Set<T>().AddRangeAsync(entities);
//        }


//        public async Task UpdateAsync<T>(T entity) where T : class
//        {
//            var dbSet = _context.Set<T>();
//            var entityEntry = _context.Entry(entity);

//            if (entityEntry.State == EntityState.Detached)
//            {
//                dbSet.Attach(entity);
//            }

//            entityEntry.State = EntityState.Modified;
//        }

//        public async Task DeleteAsync<T>(Expression<Func<T, bool>> predicate = null) where T : class
//        {
//            var dbSet = _context.Set<T>();
//            List<T> entities;

//            if (predicate == null)
//            {
//                entities = await dbSet.ToListAsync();
//            }
//            else
//            {
//                entities = await dbSet.Where(predicate).ToListAsync();
//            }

//            if (entities.Any())
//            {
//                dbSet.RemoveRange(entities);
//                await _context.SaveChangesAsync();
//            }
//        }


//        public async Task SaveChangesAsync()
//        {
//            await _context.SaveChangesAsync();
//        }

//        public async Task<bool> ExistsAsync<T>(int id) where T : class
//        {
//            return await _context.Set<T>().FindAsync(id) != null;
//        }

//        public async Task<bool> SaveAsync()
//        {
//            return await _context.SaveChangesAsync() > 0;
//        }

//        public IQueryable<T> Get<T>(Expression<Func<T, bool>> predicate) where T : class
//        {
//            return _context.Set<T>().Where(predicate);
//        }

//        public IQueryable<T> GetAllAsync<T>() where T : class
//        {
//            return _context.Set<T>();
//        }


//        public async Task<T> GetOneAsync<T>(Expression<Func<T, bool>> predicate) where T : class
//        {
//            return await _context.Set<T>().FirstOrDefaultAsync(predicate);
//        }

//        public IQueryable<T> GetAsync<T>(Expression<Func<T, bool>> predicate) where T : class
//        {
//            return _context.Set<T>().Where(predicate);
//        }

//        public async Task<T> GetByIdAsync<T>(Guid id) where T : class
//        {
//            return await _context.Set<T>().FindAsync(id);
//        }

//        public async Task<T> GetByIdAsync<T>(object id) where T : class
//        {
//            return await _context.Set<T>().FindAsync(id);
//        }

//        public async Task<IEnumerable<T>> FindAsync<T>(Expression<Func<T, bool>> predicate) where T : class
//        {
//            return await _context.Set<T>().Where(predicate).ToListAsync();
//        }

//        //public async Task<T> FirstOrDefaultAsync<T>(
//        //Expression<Func<T, bool>> predicate,
//        //Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null) where T : class
//        //{
//        //    IQueryable<T> query = _context.Set<T>().Where(predicate);
//        //    if (include != null)
//        //    {
//        //        query = include(query);
//        //    }
//        //    return await query.FirstOrDefaultAsync();
//        //}

//        public async Task<T?> FirstOrDefaultAsync<T>(
//            Expression<Func<T, bool>> predicate,
//            Func<IQueryable<T>, IQueryable<T>>? include = null,
//            CancellationToken cancellationToken = default)
//            where T : class
//                {
//                    IQueryable<T> query = _context.Set<T>();

//                    if (include != null)
//                    {
//                        query = include(query);
//                    }

//                    return await query.FirstOrDefaultAsync(predicate, cancellationToken);
//        }
//   //return await query.FirstOrDefaultAsync(predicate);
//   //         }


//        public void Add<T>(T entity) where T : class
//        {
//            _context.Set<T>().Add(entity);
//        }

//        public void Update<T>(T entity) where T : class
//        {
//            _context.Set<T>().Update(entity);
//        }

//        public void Remove<T>(T entity) where T : class
//        {
//            _context.Set<T>().Remove(entity);
//        }

//        public void RemoveRange<T>(IEnumerable<T> entities) where T : class
//        {
//            _context.Set<T>().RemoveRange(entities);
//        }



//    }
//}
