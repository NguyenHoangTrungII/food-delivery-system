using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Collections.Generic;

namespace FoodDeliverySystem.DataAccess.Contexts.Interfaces;

public interface IDbContext : IDisposable
{
    DbSet<T> Set<T>() where T : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    DatabaseFacade Database { get; }
}