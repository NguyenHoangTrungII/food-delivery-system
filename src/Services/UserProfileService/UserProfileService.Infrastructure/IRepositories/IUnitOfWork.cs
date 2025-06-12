
//using AuthService.Infrastructure.Data;
//using Microsoft.EntityFrameworkCore.Infrastructure;
//using Microsoft.EntityFrameworkCore.Storage;

//namespace AuthService.Infrastructure.IRepositories
//{

//    public interface IUnitOfWork : IDisposable
//    {
//        IGenericRepository Repository { get; }
//        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
//        Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default);
//        int SaveChanges();
//        bool SaveEntities();
//        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
//        IDbContextTransaction BeginTransaction();

//        Task CommitAsync(IDbContextTransaction transaction, CancellationToken cancellationToken = default);
//        void Commit(IDbContextTransaction transaction);

//        Task RollbackAsync(IDbContextTransaction transaction, CancellationToken cancellationToken = default);
//        void Rollback(IDbContextTransaction transaction);


//        DatabaseFacade Database { get; }
//        AuthDbContext DbContext { get; }
//    }

//    //public interface IUnitOfWork : IScopedService, IDisposable
//    //{
//    //    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
//    //    Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default);
//    //    Task<bool> SaveEntities();

//    //    IGenericRepository Repository { get; }
//    //    Task CommitTransactionAsync(IDbContextTransaction transaction);
//    //    Task RollbackTransaction();
//    //    Task RollbackTransactionAsync(IDbContextTransaction transaction);


//    //    Task<IDbContextTransaction> BeginTransactionAsync();

//    //    IDbContextTransaction BeginTransaction();
//    //    DatabaseFacade Database();
//    //    EffiRentContext DBContext();

//    //}
//}
