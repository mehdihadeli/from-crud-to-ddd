using SmartCharging.Groups.Contracts;
using SmartCharging.Shared.Application.Contracts;

namespace SmartCharging.Shared.Application.Data;

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
