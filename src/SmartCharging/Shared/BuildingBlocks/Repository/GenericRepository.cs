using Ardalis.Specification.EntityFrameworkCore;
using SmartCharging.Shared.Application.Data;
using SmartCharging.Shared.BuildingBlocks.Types;

namespace SmartCharging.Shared.BuildingBlocks.Repository;

public class GenericRepository<TAggregateRoot>(SmartChargingDbContext dbContext)
    : RepositoryBase<TAggregateRoot>(dbContext),
        IGenericRepository<TAggregateRoot>
    where TAggregateRoot : class, IAggregateRoot;
