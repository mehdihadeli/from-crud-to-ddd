using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartCharging.ServiceDefaults.Types;

namespace SmartCharging.ServiceDefaults.Repository;

public class GenericRepository<TDbContext, TAggregateRoot>(TDbContext dbContext)
    : RepositoryBase<TAggregateRoot>(dbContext),
        IGenericRepository<TDbContext, TAggregateRoot>
    where TAggregateRoot : class, IAggregateRoot
    where TDbContext : DbContext;
