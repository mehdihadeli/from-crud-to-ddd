using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace SmartChargingApi.Groups.Features.RemoveStationConnectors.v1;

public static class RemoveStationConnectorsEndpoint
{
    public static RouteHandlerBuilder MapRemoveStationConnectorsEndpoint(this RouteGroupBuilder group)
    {
        return group
            .MapDelete("/{groupId:guid}/charge-stations/{chargeStationId:guid}/connectors", Handle)
            .WithName(nameof(RemoveStationConnectors))
            .WithDisplayName(nameof(RemoveStationConnectors).Humanize())
            .WithSummary("Removes one or more connectors from a charge station in a group.")
            .WithDescription(
                "This endpoint removes one or more connectors from an existing charge station in a group while enforcing domain rules, such as ensuring the charge station always has at least one connector."
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    static async Task<Results<NoContent, ValidationProblem, ProblemHttpResult>> Handle(
        [AsParameters] RemoveStationConnectorsRequestParameters parameters
    )
    {
        var (groupId, chargeStationId, request, handler, cancellationToken) = parameters;

        var removeStationConnectors = RemoveStationConnectors.Of(
            groupId,
            chargeStationId,
            request?.ConnectorIds?.ToList()
        );
        await handler.Handle(removeStationConnectors, cancellationToken);

        return TypedResults.NoContent();
    }
}

public sealed record RemoveStationConnectorsRequestParameters(
    [FromRoute] Guid GroupId,
    [FromRoute] Guid ChargeStationId,
    [FromBody] RemoveStationConnectorsRequest? Request,
    RemoveStationConnectorsHandler Handler,
    CancellationToken CancellationToken
);

public sealed record RemoveStationConnectorsRequest(IEnumerable<int>? ConnectorIds);
