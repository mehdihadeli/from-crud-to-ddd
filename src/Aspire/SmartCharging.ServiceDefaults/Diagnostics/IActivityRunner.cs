using System.Diagnostics;

namespace SmartCharging.ServiceDefaults.Diagnostics;

public interface IActivityRunner
{
    Activity? CreateAndStartActivity(CreateActivityInfo createActivityInfo);

    Task ExecuteActivityAsync(
        CreateActivityInfo createActivityInfo,
        Func<Activity?, CancellationToken, Task> action,
        CancellationToken cancellationToken = default
    );

    Task<TResult?> ExecuteActivityAsync<TResult>(
        CreateActivityInfo createActivityInfo,
        Func<Activity?, CancellationToken, Task<TResult>> action,
        CancellationToken cancellationToken = default
    );
}
