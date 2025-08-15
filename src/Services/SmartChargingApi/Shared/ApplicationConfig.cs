using SmartCharging.ServiceDefaults.Constants;
using SmartCharging.ServiceDefaults.EF.Extensions;
using SmartCharging.ServiceDefaults.Extensions;
using SmartCharging.ServiceDefaults.Repository;
using SmartChargingApi.Groups;
using SmartChargingApi.Shared.Contracts;
using SmartChargingApi.Shared.Data;

namespace SmartChargingApi.Shared;

public static class ApplicationConfig
{
    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.AddPostgresDbContext<SmartChargingDbContext>(
            connectionStringName: AspireApplicationResources.PostgresDatabase.SmartCharging,
            action: app =>
            {
                if (app.Environment.IsDevelopment() || app.Environment.IsAspireRun())
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

        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));

        builder.AddGroupsServices();

        return builder;
    }

    public static IEndpointRouteBuilder MapApplicationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGroupsEndpoints();

        return endpoints;
    }
}
