using Ardalis.Specification;
using Microsoft.EntityFrameworkCore;
using SmartCharging.ServiceDefaults.Types;

namespace SmartCharging.ServiceDefaults.Repository;

public interface IGenericRepository<TDbContext, T> : IRepositoryBase<T>
    where T : class, IAggregateRoot
    where TDbContext : DbContext;
