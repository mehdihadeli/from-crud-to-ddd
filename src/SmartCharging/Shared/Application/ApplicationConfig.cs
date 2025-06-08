using SmartCharging.Groups;
using SmartCharging.Shared.Application.Contracts;
using SmartCharging.Shared.Application.Data;
using SmartCharging.Shared.BuildingBlocks.EF;
using SmartCharging.Shared.BuildingBlocks.Repository;

namespace SmartCharging.Shared.Application;

public static class ApplicationConfig
{
    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.AddPostgresDbContext<SmartChargingDbContext>(
            Constants.Database.SmartCharging,
            action: app =>
            {
                if (app.Environment.IsDevelopment())
                {
                    // apply migration and seed data for dev environment
                    app.AddMigrationSeedWorker<SmartChargingDbContext, SmartChargingDataSeeder>();
                }
                else
                {
                    // just apply migration for production without seeding
                    app.AddMigrationSeedWorker<SmartChargingDbContext>();
                }
            }
        );

        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        builder.AddGroupsServices();

        return builder;
    }

    public static IEndpointRouteBuilder MapApplicationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGroupsEndpoints();

        return endpoints;
    }
}
