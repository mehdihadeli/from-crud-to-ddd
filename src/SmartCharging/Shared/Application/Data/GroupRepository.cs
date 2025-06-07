using Microsoft.EntityFrameworkCore;
using SmartCharging.Groups.Contracts;
using SmartCharging.Groups.Models;
using SmartCharging.Groups.Models.ValueObjects;
using SmartCharging.Shared.BuildingBlocks.Extensions;

namespace SmartCharging.Shared.Application.Data;

public class GroupRepository(SmartChargingDbContext dbContext) : IGroupRepository
{
    public async Task<Group?> GetByIdAsync(GroupId id, CancellationToken cancellationToken = default)
    {
        id.NotBeNull();

        return await dbContext
            .Groups.Include(g => g.ChargeStations)
            .ThenInclude(cs => cs.Connectors)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Group>> GetByPageAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        pageNumber.NotBeNegativeOrZero();
        pageSize.NotBeNegativeOrZero();

        return await dbContext
            .Groups.Include(g => g.ChargeStations)
            .ThenInclude(cs => cs.Connectors)
            .OrderBy(g => g.Name.Value)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyCollection<Group>, int)> GetByPageAndTotalCountAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        pageNumber.NotBeNegativeOrZero();
        pageSize.NotBeNegativeOrZero();

        // Calculate the total count of groups
        var totalCount = await dbContext.Groups.CountAsync(cancellationToken);

        // Fetch groups for the requested page (with pagination)
        var groups = await dbContext
            .Groups.Include(g => g.ChargeStations)
            .ThenInclude(cs => cs.Connectors)
            .OrderBy(g => g.Name.Value)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (groups, totalCount);
    }

    public async Task<bool> ExistsAsync(GroupId id, CancellationToken cancellationToken = default)
    {
        id.NotBeNull();

        return await dbContext.Groups.AnyAsync(g => g.Id == id, cancellationToken);
    }

    public async Task AddAsync(Group group, CancellationToken cancellationToken = default)
    {
        group.NotBeNull();

        await dbContext.Groups.AddAsync(group, cancellationToken);
    }

    public void Update(Group group)
    {
        group.NotBeNull();

        dbContext.Groups.Attach(group);
        dbContext.Entry(group).State = EntityState.Modified;
    }

    public void Remove(Group group)
    {
        group.NotBeNull();

        dbContext.Groups.Remove(group);
    }
}
