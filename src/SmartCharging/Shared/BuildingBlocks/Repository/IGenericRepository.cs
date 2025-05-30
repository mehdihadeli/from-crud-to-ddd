using Ardalis.Specification;
using SmartCharging.Shared.BuildingBlocks.Types;

namespace SmartCharging.Shared.BuildingBlocks.Repository;

public interface IGenericRepository<T> : IRepositoryBase<T>
    where T : class, IAggregateRoot;
