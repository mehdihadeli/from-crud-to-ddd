using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartCharging.Groups.Dtos;

namespace SmartCharging.Groups.Features.CreateGroup.v1;

public static class CreateGroupEndpoint
{
    public static RouteHandlerBuilder MapCreateGroupEndpoint(this IEndpointRouteBuilder app)
    {
        return app.MapPost("/", Handle)
            .WithName(nameof(CreateGroup))
            .WithDisplayName(nameof(CreateGroup).Humanize())
            .WithSummary("Creates a new Group along with an optional Charge Station and its Connectors.")
            .WithDescription(
                "Allows the creation of a new Group providing its name and capacity. Optionally, it can include a Charge Station with associated Connectors to initialize the Group."
            )
            .Produces<CreateGroupResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        static async Task<Results<CreatedAtRoute<CreateGroupResponse>, ValidationProblem, ProblemHttpResult>> Handle(
            [AsParameters] CreateGroupRequestParameters parameters
        )
        {
            var (request, handler, cancellationToken) = parameters;

            var createGroup = CreateGroup.Of(request.Name, request.CapacityInAmps, request.ChargeStation);

            var result = await handler.Handle(createGroup, cancellationToken);

            return TypedResults.CreatedAtRoute(
                new CreateGroupResponse(result.GroupId),
                nameof(GroupGetById),
                new { groupId = result.GroupId }
            );
        }
    }
}

public record CreateGroupRequestParameters(
    [FromBody] CreateGroupRequest Request,
    CreateGroupHandler Handler,
    CancellationToken CancellationToken
);

public record CreateGroupRequest(string? Name, int CapacityInAmps, ChargeStationDto? ChargeStation = null);

// A separate type from handler result ensures the API contract is decoupled from handler logic, providing flexibility to modify the handler's response
// or internal structures without breaking the externally exposed API format.
public record CreateGroupResponse(Guid GroupId);
