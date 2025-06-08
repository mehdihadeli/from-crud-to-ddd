using Microsoft.EntityFrameworkCore;

namespace SmartCharging.Shared.BuildingBlocks.EF;

// Using IHostedService for fixing the problem for running this worker in the background with BackgroundService and running late. using IHostedService, we are ensuring our hosted-service executed before ServiceProvider resolve in the tests.
public class MigrationSeedWorker<TContext>(
    IServiceProvider serviceProvider,
    Func<TContext, IServiceProvider, Task>? seeder = null
) : IHostedService
    where TContext : DbContext
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var scopeServiceProvider = scope.ServiceProvider;
        var logger = scopeServiceProvider.GetRequiredService<ILogger<TContext>>();
        var context = scopeServiceProvider.GetRequiredService<TContext>();

        try
        {
            logger.LogInformation("Migrating database associated with context {DbContextName}", typeof(TContext).Name);

            await context.Database.MigrateAsync(cancellationToken: cancellationToken);
            if (seeder != null)
            {
                await seeder(context, scopeServiceProvider);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database migration failed for {DbContextName}", typeof(TContext).Name);

            // Throw to prevent app startup
            throw new InvalidOperationException(
                $"Database migration failed for {typeof(TContext).Name}. " + "See inner exception for details.",
                ex
            );
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
