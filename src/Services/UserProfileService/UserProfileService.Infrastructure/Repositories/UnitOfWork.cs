//using AuthService.Infrastructure.Data;
//using AuthService.Infrastructure.IRepositories;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Infrastructure;
//using Microsoft.EntityFrameworkCore.Storage;
//using System;
//using System.Collections.Generic;
//using System.Threading;
//using System.Threading.Tasks;

//namespace AuthService.Infrastructure.Repositories
//{
//    public class UnitOfWork : IUnitOfWork 
//    {
//        private readonly AuthDbContext _context;
//        private readonly Dictionary<Type, object> _repositories;

//        public UnitOfWork(AuthDbContext context)
//        {
//            _context = context ?? throw new ArgumentNullException(nameof(context));
//            _repositories = new Dictionary<Type, object>();
//        }

//        public IGenericRepository Repository
//        {
//            get
//            {
//                var type = typeof(IGenericRepository);
//                if (_repositories.ContainsKey(type))
//                    return (IGenericRepository)_repositories[type];

//                var repositoryInstance = new GenericRepository(_context);
//                _repositories.Add(type, repositoryInstance);
//                return repositoryInstance;
//            }
//        }

//        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
//        {
//            return await _context.SaveChangesAsync(cancellationToken);
//        }

//        public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
//        {
//            await _context.SaveChangesAsync(cancellationToken);
//            return true;
//        }

//        public int SaveChanges()
//        {
//            return _context.SaveChanges();
//        }

//        public bool SaveEntities()
//        {
//            _context.SaveChanges();
//            return true;
//        }

//        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
//        {
//            return await _context.Database.BeginTransactionAsync(cancellationToken);
//        }

//        public IDbContextTransaction BeginTransaction()
//        {
//            return _context.Database.BeginTransaction();
//        }

//        public async Task CommitAsync(IDbContextTransaction transaction, CancellationToken cancellationToken = default)
//        {
//            if (transaction == null)
//                throw new ArgumentNullException(nameof(transaction));

//            await transaction.CommitAsync(cancellationToken);
//        }

//        public void Commit(IDbContextTransaction transaction)
//        {
//            if (transaction == null)
//                throw new ArgumentNullException(nameof(transaction));

//            transaction.Commit();
//        }

//        public async Task RollbackAsync(IDbContextTransaction transaction, CancellationToken cancellationToken = default)
//        {
//            if (transaction == null)
//                throw new ArgumentNullException(nameof(transaction));

//            await transaction.RollbackAsync(cancellationToken);
//        }

//        public void Rollback(IDbContextTransaction transaction)
//        {
//            if (transaction == null)
//                throw new ArgumentNullException(nameof(transaction));

//            transaction.Rollback();
//        }


//        public DatabaseFacade Database => _context.Database;

//        public AuthDbContext DbContext => _context;

//        public void Dispose()
//        {
//            // Không dispose _context, để DI container hoặc lớp kiểm thử quản lý
//            // Chỉ cleanup tài nguyên nội bộ nếu cần (hiện tại không có)
//            GC.SuppressFinalize(this);
//        }
//    }
//}
