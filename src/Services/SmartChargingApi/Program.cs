using SmartCharging.ServiceDefaults.Extensions;
using SmartCharging.Shared;
using SmartChargingApi.Shared;
using SmartChargingApi.Shared.Extensions.WebApplicationBuilderExtensions;
using SmartChargingApi.Shared.Extensions.WebApplicationExtensions;

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
