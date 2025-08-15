using Humanizer;
using Microsoft.Extensions.ServiceDiscovery;
using SmartCharging.ServiceDefaults.Constants;
using SmartChargingApi.Groups.Contracts;
using SmartChargingApi.Groups.Data;
using SmartChargingApi.Groups.Features.AddChargeStation.v1;
using SmartChargingApi.Groups.Features.AddStationConnectors.v1;
using SmartChargingApi.Groups.Features.CreateGroup.v1;
using SmartChargingApi.Groups.Features.GetGroupById.v1;
using SmartChargingApi.Groups.Features.GetGroupsByPage.v1;
using SmartChargingApi.Groups.Features.RemoveChargeStation.v1;
using SmartChargingApi.Groups.Features.RemoveGroup.v1;
using SmartChargingApi.Groups.Features.RemoveStationConnectors.v1;
using SmartChargingApi.Groups.Features.UpdateChargeStationName.v1;
using SmartChargingApi.Groups.Features.UpdateConnectorCurrentInAmps.v1;
using SmartChargingApi.Groups.Features.UpdateGroup.v1;
using SmartChargingApi.Groups.Services;

namespace SmartChargingApi.Groups;

public static class GroupsConfig
{
    public static WebApplicationBuilder AddGroupsServices(this WebApplicationBuilder builder)
    {
        AddHandlers(builder);

        builder.Services.AddScoped<IGroupRepository, GroupRepository>();
        builder.Services.AddHttpClient<IGroupStatisticsExternalProvider, GroupStatisticsExternalProvider>(
            (sp, client) =>
            {
                var resolver = sp.GetRequiredService<ServiceEndpointResolver>();
                var endpoints = resolver
                    .GetEndpointsAsync(
                        $"https+http://{AspireApplicationResources.Api.SmartChargingStatisticsApi}",
                        CancellationToken.None
                    )
                    .GetAwaiter()
                    .GetResult();
                endpoints.Endpoints.First();

                // https://learn.microsoft.com/en-us/dotnet/aspire/service-discovery/overview
                // https://learn.microsoft.com/en-us/dotnet/core/extensions/service-discovery?tabs=dotnet-cli#example-usage
                // Logical name, resolved by service discovery support for an http client
                client.BaseAddress = new Uri(
                    // smart-charging-statistics-api
                    $"https+http://{AspireApplicationResources.Api.SmartChargingStatisticsApi}"
                );
            }
        );

        return builder;
    }

    public static IEndpointRouteBuilder MapGroupsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var groups = endpoints.NewVersionedApi(nameof(Groups).Pluralize().Kebaberize());
        var groupV1 = groups.MapGroup("/api/v{version:apiVersion}/groups").HasApiVersion(1.0);

        groupV1.MapCreateGroupEndpoint();
        groupV1.MapUpdateGroupEndpoint();
        groupV1.MapRemoveGroupEndpoint();
        groupV1.MapAddChargeStationEndpoint();
        groupV1.MapRemoveChargeStationEndpoint();
        groupV1.MapUpdateChargeStationNameEndpoint();
        groupV1.MapAddStationConnectorsEndpoint();
        groupV1.MapRemoveStationConnectorsEndpoint();
        groupV1.MapUpdateConnectorCurrentInAmpsEndpoint();
        groupV1.MapGetGroupByIdEndpoint();
        groupV1.MapGetGroupsByPageEndpoint();

        return endpoints;
    }

    private static void AddHandlers(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<CreateGroupHandler>();
        builder.Services.AddScoped<UpdateGroupHandler>();
        builder.Services.AddScoped<RemoveGroupHandler>();
        builder.Services.AddScoped<AddChargeStationHandler>();
        builder.Services.AddScoped<RemoveChargeStationHandler>();
        builder.Services.AddScoped<UpdateChargeStationNameHandler>();
        builder.Services.AddScoped<AddConnectorsHandler>();
        builder.Services.AddScoped<RemoveStationConnectorsHandler>();
        builder.Services.AddScoped<UpdateConnectorCurrentInAmpsHandler>();
        builder.Services.AddScoped<GetGroupByIdHandler>();
        builder.Services.AddScoped<GetGroupsByPageHandler>();
    }
}
