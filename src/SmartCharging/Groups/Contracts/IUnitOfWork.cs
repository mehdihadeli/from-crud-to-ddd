namespace SmartCharging.Groups.Contracts;

public interface IUnitOfWork : IDisposable
{
    IGroupRepository GroupRepository { get; }

    Task<int> CommitAsync(CancellationToken cancellationToken = default);
}
