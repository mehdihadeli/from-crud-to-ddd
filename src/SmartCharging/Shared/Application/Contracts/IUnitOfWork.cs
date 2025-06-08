using SmartCharging.Groups.Contracts;

namespace SmartCharging.Shared.Application.Contracts;

public interface IUnitOfWork : IDisposable
{
    IGroupRepository GroupRepository { get; }

    Task<int> CommitAsync(CancellationToken cancellationToken = default);
}
