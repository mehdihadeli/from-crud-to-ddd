using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartCharging.Groups.Dtos;

namespace SmartCharging.Groups.Features.AddStationConnectors.v1;

public static class AddStationConnectorsEndpoint
{
    public static RouteHandlerBuilder MapAddStationConnectorsEndpoint(this RouteGroupBuilder group)
    {
        return group
            .MapPost("/{groupId:guid}/charge-stations/{chargeStationId:guid}/connectors", Handle)
            .WithName(nameof(AddStationConnectors))
            .WithDisplayName("Add Connector(s)")
            .WithSummary("Adds one or more connectors to a charge station in a group.")
            .WithDescription(
                "This endpoint allows adding one or more connectors to an existing charge station in a group. Validations are enforced for domain rules, such as max group capacity and max number of connectors per station."
            )
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        static async Task<Results<CreatedAtRoute<AddConnectorResponse>, ValidationProblem, ProblemHttpResult>> Handle(
            [AsParameters] AddConnectorRequestParameters parameters
        )
        {
            var (groupId, chargeStationId, request, handler, cancellationToken) = parameters;

            var addConnector = AddStationConnectors.Of(
                groupId,
                chargeStationId,
                request.Connectors?.ToList().AsReadOnly()
            );
            var result = await handler.Handle(addConnector, cancellationToken);

            var response = new AddConnectorResponse(groupId, chargeStationId, result.Connectors);

            return TypedResults.CreatedAtRoute(
                response,
                nameof(AddStationConnectors),
                new { groupId, chargeStationId }
            );
        }
    }
}

public record AddConnectorRequestParameters(
    [FromRoute] Guid GroupId,
    [FromRoute] Guid ChargeStationId,
    [FromBody] AddConnectorRequest Request,
    AddConnectorsHandler Handler,
    CancellationToken CancellationToken
);

public record AddConnectorRequest(IEnumerable<ConnectorDto>? Connectors);

// A separate response type from handler result, ensures the API contract is decoupled from handler logic, providing flexibility to modify the handler's response
// or internal structures without breaking the externally exposed API format.
public record AddConnectorResponse(Guid GroupId, Guid ChargeStationId, IReadOnlyCollection<ConnectorDto> Connectors);
