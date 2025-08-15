using SmartCharging.ServiceDefaults.Types;

namespace SmartCharging.ServiceDefaults.Repository;

public interface IRepository<T>
    where T : IAggregateRoot;
