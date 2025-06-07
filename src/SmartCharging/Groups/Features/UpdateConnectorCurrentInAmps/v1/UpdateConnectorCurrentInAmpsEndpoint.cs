using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartCharging.Shared.BuildingBlocks.Extensions;

namespace SmartCharging.Groups.Features.UpdateConnectorCurrentInAmps.v1;

public static class UpdateConnectorCurrentInAmpsEndpoint
{
    public static RouteHandlerBuilder MapUpdateConnectorCurrentInAmpsEndpoint(this RouteGroupBuilder group)
    {
        return group
            .MapPut(
                "/{groupId:guid}/charge-stations/{chargeStationId:guid}/connectors/{connectorId:int}/current",
                HandleAsync
            )
            .WithName(nameof(UpdateConnectorCurrentInAmps))
            .WithDisplayName(nameof(UpdateConnectorCurrentInAmps).Humanize())
            .WithSummary("Updates the maximum current in amps for a connector in a charge station within a group.")
            .WithDescription(
                "This endpoint updates the maximum current (in amps) for a specific connector in a charge station within a group. The request validates group capacity, ensuring the new current value doesn't violate capacity rules."
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    static async Task<Results<NoContent, ValidationProblem, ProblemHttpResult>> HandleAsync(
        [AsParameters] UpdateConnectorCurrentInAmpsRequestParameters parameters
    )
    {
        var (groupId, chargeStationId, connectorId, request, handler, cancellationToken) = parameters;

        var updateConnectorCurrentInAmps = UpdateConnectorCurrentInAmps.Of(
            groupId,
            chargeStationId,
            connectorId,
            request?.NewCurrentInAmps ?? 0
        );

        await handler.Handle(updateConnectorCurrentInAmps, cancellationToken);

        return TypedResults.NoContent();
    }
}

public sealed record UpdateConnectorCurrentInAmpsRequestParameters(
    [FromRoute] Guid GroupId,
    [FromRoute] Guid ChargeStationId,
    [FromRoute] int ConnectorId,
    [FromBody] UpdateConnectorCurrentInAmpsRequest? Request,
    UpdateConnectorCurrentInAmpsHandler Handler,
    CancellationToken CancellationToken
);

public sealed record UpdateConnectorCurrentInAmpsRequest(int NewCurrentInAmps);
