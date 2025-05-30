using Microsoft.EntityFrameworkCore;

namespace SmartCharging.Shared.BuildingBlocks.EF;

internal class MigrationSeedWorker<TContext>(
    IServiceProvider serviceProvider,
    Func<TContext, IServiceProvider, Task>? seeder = null
) : BackgroundService
    where TContext : DbContext
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        var scopeServiceProvider = scope.ServiceProvider;
        var logger = scopeServiceProvider.GetRequiredService<ILogger<TContext>>();
        var context = scopeServiceProvider.GetRequiredService<TContext>();

        try
        {
            logger.LogInformation("Migrating database associated with context {DbContextName}", typeof(TContext).Name);

            await context.Database.MigrateAsync(cancellationToken: stoppingToken);
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
}

public interface IDataSeeder<in TContext>
    where TContext : DbContext
{
    Task SeedAsync(TContext context);
}
