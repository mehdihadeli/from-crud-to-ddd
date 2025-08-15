using SmartChargingApi.Groups.Contracts;
using SmartChargingApi.Shared.Contracts;

namespace SmartChargingApi.Shared.Data;

public sealed class UnitOfWork(SmartChargingDbContext dbContext, IGroupRepository groupRepository) : IUnitOfWork
{
    private bool _disposed;

    public IGroupRepository GroupRepository => groupRepository;

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                dbContext.Dispose();
            }
            _disposed = true;
        }
    }
}
