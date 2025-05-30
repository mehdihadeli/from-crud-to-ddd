using SmartCharging.Groups;
using SmartCharging.Groups.Contracts;
using SmartCharging.Groups.Models;
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
                    app.AddMigration<SmartChargingDbContext, SmartChargingDataSeeder>();
                }
                else
                {
                    // just apply migration for production without seeding
                    app.AddMigration<SmartChargingDbContext>();
                }
            }
        );

        builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        // Add all application custom-typed repositories
        builder.AddRepositories(typeof(ChargeStation).Assembly);

        builder.AddGroupsServices();

        return builder;
    }

    public static IEndpointRouteBuilder MapApplicationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGroupsEndpoints();

        return endpoints;
    }
}
