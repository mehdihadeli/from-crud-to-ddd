using SmartCharging.Groups.Models;
using SmartCharging.Groups.Models.ValueObjects;
using SmartCharging.Shared.BuildingBlocks.Repository;

namespace SmartCharging.Groups.Contracts;

public interface IGroupRepository : IRepository<Group>
{
    Task<Group?> GetByIdAsync(GroupId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Group>> GetGroupsByPageAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    Task<(IReadOnlyCollection<Group>, int)> GetGroupsAndTotalCountAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    );
    Task<bool> ExistsAsync(GroupId id, CancellationToken cancellationToken = default);
    Task AddAsync(Group group, CancellationToken cancellationToken = default);
    void Update(Group group);
    void Remove(Group group);
}
