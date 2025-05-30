using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace SmartCharging.Groups.Features.RemoveChargeStation.v1;

public static class RemoveChargeStationEndpoint
{
    public static RouteHandlerBuilder MapRemoveChargeStationEndpoint(this RouteGroupBuilder group)
    {
        return group
            .MapDelete("/{groupId:guid}/charge-stations/{chargeStationId:guid}", Handle)
            .WithName(nameof(RemoveChargeStation))
            .WithDisplayName(nameof(RemoveChargeStation).Humanize())
            .WithSummary("Removes a charge station from a group.")
            .WithDescription("Removes a specific charge station from a group.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();

        static async Task<Results<NoContent, ProblemHttpResult>> Handle(
            [AsParameters] RemoveChargeStationRequestParameters parameters
        )
        {
            var (groupId, chargeStationId, handler, cancellationToken) = parameters;

            var removeChargeStation = RemoveChargeStation.Of(groupId, chargeStationId);
            await handler.Handle(removeChargeStation, cancellationToken);

            return TypedResults.NoContent();
        }
    }
}

public record RemoveChargeStationRequestParameters(
    [FromRoute] Guid GroupId,
    [FromRoute] Guid ChargeStationId,
    RemoveChargeStationHandler Handler,
    CancellationToken CancellationToken
);
