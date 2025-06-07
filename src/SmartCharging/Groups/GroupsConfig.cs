using Humanizer;
using SmartCharging.Groups.Contracts;
using SmartCharging.Groups.Features.AddChargeStation.v1;
using SmartCharging.Groups.Features.AddStationConnectors.v1;
using SmartCharging.Groups.Features.CreateGroup.v1;
using SmartCharging.Groups.Features.GetGroupsByPage.v1;
using SmartCharging.Groups.Features.GroupGetById.v1;
using SmartCharging.Groups.Features.RemoveChargeStation.v1;
using SmartCharging.Groups.Features.RemoveGroup.v1;
using SmartCharging.Groups.Features.RemoveStationConnectors.v1;
using SmartCharging.Groups.Features.UpdateChargeStationName.v1;
using SmartCharging.Groups.Features.UpdateConnectorCurrentInAmps.v1;
using SmartCharging.Groups.Features.UpdateGroup.v1;
using SmartCharging.Shared.Application.Data;

namespace SmartCharging.Groups;

public static class GroupsConfig
{
    public static WebApplicationBuilder AddGroupsServices(this WebApplicationBuilder builder)
    {
        AddHandlers(builder);

        builder.Services.AddScoped<IGroupRepository, GroupRepository>();

        return builder;
    }

    public static IEndpointRouteBuilder MapGroupsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var groups = endpoints.NewVersionedApi(nameof(Groups).Pluralize());
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
        groupV1.MapGroupGetByIdEndpoint();
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
        builder.Services.AddScoped<GroupGetByIdHandler>();
        builder.Services.AddScoped<GetGroupsByPageHandler>();
    }
}
