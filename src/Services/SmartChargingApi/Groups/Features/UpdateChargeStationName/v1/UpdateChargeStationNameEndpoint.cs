using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace SmartChargingApi.Groups.Features.UpdateChargeStationName.v1;

public static class UpdateChargeStationNameEndpoint
{
    public static RouteHandlerBuilder MapUpdateChargeStationNameEndpoint(this RouteGroupBuilder group)
    {
        return group
            .MapPut("/{groupId:guid}/charge-stations/{chargeStationId:guid}/name", HandleAsync)
            .WithName(nameof(UpdateChargeStationName))
            .WithDisplayName(nameof(UpdateChargeStationName).Humanize())
            .WithSummary("Updates the name of a charge station in a group.")
            .WithDescription(
                "This endpoint updates the name of a specific charge station within a group. The group and charge station must exist, and the name cannot be empty."
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [AsParameters] UpdateChargeStationNameRequestParameters parameters
    )
    {
        var (groupId, chargeStationId, request, handler, cancellationToken) = parameters;

        var updateChargeStationName = UpdateChargeStationName.Of(groupId, chargeStationId, request?.NewName);
        await handler.Handle(updateChargeStationName, cancellationToken);

        return TypedResults.NoContent();
    }
}

public sealed record UpdateChargeStationNameRequestParameters(
    [FromRoute] Guid GroupId,
    [FromRoute] Guid ChargeStationId,
    [FromBody] UpdateChargeStationNameRequest? Request,
    UpdateChargeStationNameHandler Handler,
    CancellationToken CancellationToken
);

public sealed record UpdateChargeStationNameRequest(string NewName);
