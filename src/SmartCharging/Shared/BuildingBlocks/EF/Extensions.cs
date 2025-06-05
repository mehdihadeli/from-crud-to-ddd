using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace SmartCharging.Shared.BuildingBlocks.EF;

public static class Extensions
{
    public static void AddPostgresDbContext<TDbContext>(
        this WebApplicationBuilder builder,
        string name,
        Action<WebApplicationBuilder>? action = null
    )
        where TDbContext : DbContext
    {
        var services = builder.Services;

        services.AddDbContext<TDbContext>(
            (sp, options) =>
            {
                options
                    .UseNpgsql(
                        builder.Configuration.GetConnectionString(name),
                        sqlOptions =>
                        {
                            var assemblyName = typeof(TDbContext).Assembly.GetName().Name;

                            sqlOptions.MigrationsAssembly(assemblyName);
                            sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                        }
                    )
                    .UseSnakeCaseNamingConvention()
                    .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
            }
        );

        action?.Invoke(builder);
    }

    public static WebApplicationBuilder AddMigration<TContext>(
        this WebApplicationBuilder builder,
        Func<TContext, IServiceProvider, Task>? seeder = null
    )
        where TContext : DbContext
    {
        builder.Services.AddHostedService(sp => new MigrationSeedWorker<TContext>(sp, seeder));

        return builder;
    }

    public static WebApplicationBuilder AddMigration<TContext, TDbSeeder>(this WebApplicationBuilder builder)
        where TContext : DbContext
        where TDbSeeder : class, IDataSeeder<TContext>
    {
        builder.Services.AddScoped<IDataSeeder<TContext>, TDbSeeder>();

        return builder.AddMigration<TContext>(
            (context, sp) => sp.GetRequiredService<IDataSeeder<TContext>>().SeedAsync(context)
        );
    }
}
