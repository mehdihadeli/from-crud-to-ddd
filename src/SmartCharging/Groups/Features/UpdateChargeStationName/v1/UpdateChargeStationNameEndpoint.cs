namespace SmartCharging.Groups.Features.UpdateChargeStationName.v1;

using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

public static class UpdateChargeStationNameEndpoint
{
    public static RouteHandlerBuilder MapUpdateChargeStationNameEndpoint(this RouteGroupBuilder group)
    {
        return group
            .MapPut("/{groupId:guid}/charge-stations/{chargeStationId:guid}/name", Handle)
            .WithName(nameof(UpdateChargeStationName))
            .WithDisplayName(nameof(UpdateChargeStationName).Humanize())
            .WithSummary("Updates the name of a charge station in a group.")
            .WithDescription(
                "This endpoint updates the name of a specific charge station within a group. The group and charge station must exist, and the name cannot be empty."
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        static async Task<Results<NoContent, ProblemHttpResult>> Handle(
            [AsParameters] UpdateChargeStationNameRequestParameters parameters
        )
        {
            var (groupId, chargeStationId, request, handler, cancellationToken) = parameters;

            var updateChargeStationName = UpdateChargeStationName.Of(groupId, chargeStationId, request.NewName);
            await handler.Handle(updateChargeStationName, cancellationToken);

            return TypedResults.NoContent();
        }
    }
}

public record UpdateChargeStationNameRequestParameters(
    [FromRoute] Guid GroupId,
    [FromRoute] Guid ChargeStationId,
    [FromBody] UpdateChargeStationNameRequest Request,
    UpdateChargeStationNameHandler Handler,
    CancellationToken CancellationToken
);

public record UpdateChargeStationNameRequest(string NewName);
