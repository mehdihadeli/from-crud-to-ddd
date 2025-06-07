using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace SmartCharging.Groups.Features.AddChargeStation.v1;

public static class AddChargeStationEndpoint
{
    public static RouteHandlerBuilder MapAddChargeStationEndpoint(this IEndpointRouteBuilder app)
    {
        return app.MapPost("/{groupId:guid}/charge-stations", HandleAsync)
            .WithName(nameof(AddChargeStation))
            .WithDisplayName(nameof(AddChargeStation).Humanize())
            .WithSummary("Adds a single new charge station to a group.")
            .WithDescription(
                "This endpoint allows the addition of exactly one charge station to a group. "
                    + "The station must have at least one connector to be valid."
            )
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    static async Task<Results<Created<AddChargeStationResponse>, ProblemHttpResult, ValidationProblem>> HandleAsync(
        [AsParameters] AddChargeStationRequestParameters parameters
    )
    {
        var (groupId, request, handler, cancellationToken) = parameters;

        var addChargeStation = AddChargeStation.Of(
            groupId,
            request?.ChargeStationId,
            request?.Name,
            request?.ConnectorsRequest?.ToConnectorsDto(request.ChargeStationId)
        );

        var chargeStationId = await handler.Handle(addChargeStation, cancellationToken);

        return TypedResults.Created(
            $"/api/groups/{groupId}/charge-stations/{chargeStationId}",
            new AddChargeStationResponse(chargeStationId)
        );
    }
}

public sealed record AddChargeStationRequestParameters(
    [FromRoute] Guid GroupId,
    [FromBody] AddChargeStationRequest? Request,
    AddChargeStationHandler Handler,
    CancellationToken CancellationToken
);

public sealed record AddChargeStationRequest(
    string? Name,
    IEnumerable<AddChargeStationRequest.CreateConnectorRequest>? ConnectorsRequest
)
{
    // make it internal to prevent exposing in openapi schema
    internal Guid ChargeStationId { get; } = Guid.CreateVersion7();

    public sealed record CreateConnectorRequest(int ConnectorId, int MaxCurrentInAmps);
}

public sealed record AddChargeStationResponse(Guid ChargeStationId);
