using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartCharging.Groups.Dtos;

namespace SmartCharging.Groups.Features.AddStationConnectors.v1;

public static class AddStationConnectorsEndpoint
{
    public static RouteHandlerBuilder MapAddStationConnectorsEndpoint(this RouteGroupBuilder group)
    {
        return group
            .MapPost("/{groupId:guid}/charge-stations/{chargeStationId:guid}/connectors", HandleAsync)
            .WithName(nameof(AddStationConnectors))
            .WithDisplayName("Add Connector(s)")
            .WithSummary("Adds one or more connectors to a charge station in a group.")
            .WithDescription(
                "This endpoint allows adding one or more connectors to an existing charge station in a group. Validations are enforced for domain rules, such as max group capacity and max number of connectors per station."
            )
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    static async Task<Results<Created<AddConnectorResponse>, ValidationProblem, ProblemHttpResult>> HandleAsync(
        [AsParameters] AddConnectorRequestParameters parameters
    )
    {
        var (groupId, chargeStationId, request, handler, cancellationToken) = parameters;

        var addConnector = AddStationConnectors.Of(
            groupId,
            chargeStationId,
            request?.ConnectorsRequest?.ToConnectorsDto(chargeStationId)
        );
        var result = await handler.Handle(addConnector, cancellationToken);

        var response = new AddConnectorResponse(groupId, chargeStationId, result.Connectors);

        return TypedResults.Created(string.Empty, response);
    }
}

public sealed record AddConnectorRequestParameters(
    [FromRoute] Guid GroupId,
    [FromRoute] Guid ChargeStationId,
    [FromBody] AddConnectorRequest? Request,
    AddConnectorsHandler Handler,
    CancellationToken CancellationToken
);

public sealed record AddConnectorRequest(IEnumerable<AddConnectorRequest.CreateConnectorRequest>? ConnectorsRequest)
{
    public sealed record CreateConnectorRequest(int ConnectorId, int MaxCurrentInAmps);
}

// A separate response type from handler result ensures the API contract is decoupled from handler logic, providing flexibility to modify the handler's response
// or internal structures without breaking the externally exposed API format.
public sealed record AddConnectorResponse(
    Guid GroupId,
    Guid ChargeStationId,
    IReadOnlyCollection<ConnectorDto> Connectors
);
