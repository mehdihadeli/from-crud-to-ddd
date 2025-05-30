using SmartCharging.Shared.BuildingBlocks.Types;

namespace SmartCharging.Shared.BuildingBlocks.Repository;

public interface IRepository<T>
    where T : IAggregateRoot;
