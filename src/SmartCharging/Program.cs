using SmartCharging.Shared.Application;
using SmartCharging.Shared.Application.Extensions.WebApplicationBuilderExtensions;
using SmartCharging.Shared.Application.Extensions.WebApplicationExtensions;
using SmartCharging.Shared.BuildingBlocks.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.AddInfrastructure();

builder.AddApplicationServices();

var app = builder.Build();

app.UseInfrastructure();

app.MapApplicationEndpoints();

app.UseAspnetOpenApi();

await app.RunAsync();
