using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SmartCharging.Shared.Application.Contracts;

namespace SmartCharging.Shared.BuildingBlocks.EF;

public static class Extensions
{
    public static void AddPostgresDbContext<TDbContext>(
        this WebApplicationBuilder builder,
        string connectionName,
        Action<WebApplicationBuilder>? action = null
    )
        where TDbContext : DbContext
    {
        builder.Services.AddDbContext<TDbContext>(
            (sp, options) =>
            {
                options
                    .UseNpgsql(
                        builder.Configuration.GetConnectionString(connectionName),
                        sqlOptions =>
                        {
                            var assemblyName = typeof(TDbContext).Assembly.GetName().Name;

                            sqlOptions.MigrationsAssembly(assemblyName);
                            sqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(10), null);
                        }
                    )
                    .UseSnakeCaseNamingConvention()
                    .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));

                options.ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector<Guid>>();
            }
        );

        action?.Invoke(builder);
    }

    public static WebApplicationBuilder AddMigrationSeedWorker<TContext>(
        this WebApplicationBuilder builder,
        Func<TContext, IServiceProvider, Task>? seeder = null
    )
        where TContext : DbContext
    {
        builder.Services.AddHostedService(sp => new MigrationSeedWorker<TContext>(sp, seeder));

        return builder;
    }

    public static WebApplicationBuilder AddMigrationSeedWorker<TContext, TDbSeeder>(this WebApplicationBuilder builder)
        where TContext : DbContext
        where TDbSeeder : class, IDataSeeder<TContext>
    {
        builder.Services.AddScoped<IDataSeeder<TContext>, TDbSeeder>();

        return builder.AddMigrationSeedWorker<TContext>(
            (context, sp) => sp.GetRequiredService<IDataSeeder<TContext>>().SeedAsync(context)
        );
    }
}
