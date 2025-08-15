using SmartChargingApi.Groups.Contracts;

namespace SmartChargingApi.Shared.Contracts;

public interface IUnitOfWork : IDisposable
{
    IGroupRepository GroupRepository { get; }

    Task<int> CommitAsync(CancellationToken cancellationToken = default);
}
