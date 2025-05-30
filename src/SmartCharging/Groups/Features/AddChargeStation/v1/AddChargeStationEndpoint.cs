using Humanizer;
using Microsoft.AspNetCore.Mvc;
using SmartCharging.Groups.Dtos;

namespace SmartCharging.Groups.Features.AddChargeStation.v1;

public static class AddChargeStationEndpoint
{
    public static RouteGroupBuilder MapAddChargeStationEndpoint(this RouteGroupBuilder group)
    {
        group
            .MapPost(
                "/{groupId:guid}/charge-stations",
                async (
                    [FromRoute] Guid groupId,
                    [FromBody] AddChargeStationRequest request,
                    AddChargeStationHandler handler,
                    CancellationToken cancellationToken
                ) =>
                {
                    var addChargeStation = AddChargeStation.Of(
                        groupId,
                        request.ChargeStationId,
                        request.Name,
                        request.Connectors?.ToList().AsReadOnly()
                    );
                    await handler.Handle(addChargeStation, cancellationToken);

                    return Results.Created($"/api/groups/{groupId}/charge-stations/{request.ChargeStationId}", null);
                }
            )
            .WithName(nameof(AddChargeStation))
            .WithDisplayName(nameof(AddChargeStation).Humanize())
            .WithSummary("Adds a single new charge station to a group.")
            .WithDescription(
                "This endpoint allows the addition of exactly one charge station to a group. The station must have at least one connector to be valid."
            )
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }
}

public record AddChargeStationRequest(Guid? ChargeStationId, string? Name, IEnumerable<ConnectorDto>? Connectors);
