using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartChargingApi.Groups.Dtos.Response;

namespace SmartChargingApi.Groups.Features.GetGroupById.v1;

public static class GetGroupByIdEndpoint
{
    public static RouteHandlerBuilder MapGetGroupByIdEndpoint(this RouteGroupBuilder group)
    {
        return group
            .MapGet("/{groupId:guid}", HandleAsync)
            .WithName(nameof(GetGroupById))
            .WithDisplayName(nameof(GetGroupById).Humanize())
            .WithSummary("Retrieves the details of a Group by its ID.")
            .WithDescription(
                "This endpoint fetches details of a specific Group using its unique identifier. It includes information about the Group, including its associated Charge Stations and Connectors."
            )
            .Produces<GroupGetByIdResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    static async Task<Results<Ok<GetGroupByIdResponse>, ProblemHttpResult>> HandleAsync(
        [AsParameters] GroupGetByIdRequestParameters parameters
    )
    {
        var (groupId, handler, cancellationToken) = parameters;

        var getById = GetGroupById.Of(groupId);
        var result = await handler.Handle(getById, cancellationToken);

        var response = new GetGroupByIdResponse(
            result.Group.GroupId,
            result.Group.Name,
            result.Group.CapacityInAmps,
            result.Group.ChargeStations.Select(x => x.ToChargeStationResponse()),
            result.EnergyStats?.ToGroupEnergyConsumptionResponse(),
            result.CapacityStats?.ToGroupCapacityStatisticsResponse()
        );

        return TypedResults.Ok(response);
    }
}

public sealed record GroupGetByIdRequestParameters(
    [FromRoute] Guid GroupId,
    GetGroupByIdHandler Handler,
    CancellationToken CancellationToken
);

// A separate response type from handler result ensures the API contract is decoupled from handler logic, providing flexibility to modify the handler's response
// or internal structures without breaking the externally exposed API format.
public sealed record GetGroupByIdResponse(
    Guid GroupId,
    string Name,
    int CapacityInAmps,
    IEnumerable<ChargeStationResponseDto> ChargeStations,
    GroupEnergyConsumptionResponseDto? EnergyStats,
    GroupCapacityStatisticsResponseDto? CapacityStats
);
