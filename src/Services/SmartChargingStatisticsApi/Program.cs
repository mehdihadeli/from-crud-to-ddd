using SmartCharging.ServiceDefaults.Extensions;
using SmartChargingStatisticsApi.Shared;
using SmartChargingStatisticsApi.Shared.Extensions.WebApplicationBuilderExtensions;
using SmartChargingStatisticsApi.Shared.Extensions.WebApplicationExtensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddInfrastructure();

builder.AddApplicationServices();

var app = builder.Build();

app.UseDefaultServices();

app.UseInfrastructure();

app.MapControllers();

app.MapDefaultEndpoints();

app.MapApplicationEndpoints();

app.Run();
