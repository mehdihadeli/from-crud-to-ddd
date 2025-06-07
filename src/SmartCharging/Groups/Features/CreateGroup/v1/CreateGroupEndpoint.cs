using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace SmartCharging.Groups.Features.CreateGroup.v1;

public static class CreateGroupEndpoint
{
    public static RouteHandlerBuilder MapCreateGroupEndpoint(this IEndpointRouteBuilder app)
    {
        return app.MapPost("/", HandleAsync)
            .WithName(nameof(CreateGroup))
            .WithDisplayName(nameof(CreateGroup).Humanize())
            .WithSummary("Creates a new Group along with an optional Charge Station and its Connectors.")
            .WithDescription(
                "Allows the creation of a new Group providing its name and capacity. Optionally, it can include a Charge Station with associated Connectors to initialize the Group."
            )
            .Produces<CreateGroupResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();
    }

    static async Task<Results<CreatedAtRoute<CreateGroupResponse>, ValidationProblem, ProblemHttpResult>> HandleAsync(
        [AsParameters] CreateGroupRequestParameters parameters
    )
    {
        var (request, handler, cancellationToken) = parameters;

        var createGroup = CreateGroup.Of(
            request?.Name,
            request?.CapacityInAmps,
            request?.ChargeStationRequest?.ToChargeStationDto()
        );

        var result = await handler.Handle(createGroup, cancellationToken);

        return TypedResults.CreatedAtRoute(
            new CreateGroupResponse(result.GroupId),
            nameof(GroupGetById),
            new { groupId = result.GroupId }
        );
    }
}

public sealed record CreateGroupRequestParameters(
    [FromBody] CreateGroupRequest? Request,
    CreateGroupHandler Handler,
    CancellationToken CancellationToken
);

// A separate request type from handler parameters ensures the API contract is decoupled from handler logic, providing flexibility to modify the handler's request.
public sealed record CreateGroupRequest(
    string? Name,
    int CapacityInAmps,
    CreateGroupRequest.CreateChargeStationRequest? ChargeStationRequest = null
)
{
    public sealed record CreateChargeStationRequest(string Name, IReadOnlyCollection<CreateConnectorRequest> Connectors)
    {
        // make it internal to prevent exposing in openapi schema
        internal Guid ChargeStationId { get; } = Guid.CreateVersion7();
    }

    public sealed record CreateConnectorRequest(int ConnectorId, int MaxCurrentInAmps);
}

// A separate type from handler result ensures the API contract is decoupled from handler logic, providing flexibility to modify the handler's response
// or internal structures without breaking the externally exposed API format.
public sealed record CreateGroupResponse(Guid GroupId);
