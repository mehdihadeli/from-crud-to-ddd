using SmartCharging.ServiceDefaults.Repository;
using SmartChargingApi.Groups.Models;
using SmartChargingApi.Groups.Models.ValueObjects;

namespace SmartChargingApi.Groups.Contracts;

public interface IGroupRepository : IRepository<Group>
{
    Task<Group?> GetByIdAsync(GroupId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Group>> GetByPageAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    Task<(IReadOnlyCollection<Group>, int)> GetByPageAndTotalCountAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    );
    Task<bool> ExistsAsync(GroupId id, CancellationToken cancellationToken = default);
    Task AddAsync(Group group, CancellationToken cancellationToken = default);
    void Update(Group group);
    void Remove(Group group);
}
