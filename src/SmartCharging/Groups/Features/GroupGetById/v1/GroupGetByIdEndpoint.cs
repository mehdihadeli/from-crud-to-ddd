using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartCharging.Groups.Dtos;

namespace SmartCharging.Groups.Features.GroupGetById.v1;

public static class GroupGetByIdEndpoint
{
    public static RouteHandlerBuilder MapGroupGetByIdEndpoint(this RouteGroupBuilder group)
    {
        return group
            .MapGet("/{groupId:guid}", HandleAsync)
            .WithName(nameof(GroupGetById))
            .WithDisplayName(nameof(GroupGetById).Humanize())
            .WithSummary("Retrieves the details of a Group by its ID.")
            .WithDescription(
                "This endpoint fetches details of a specific Group using its unique identifier. It includes information about the Group, including its associated Charge Stations and Connectors."
            )
            .Produces<GroupGetByIdResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    static async Task<Results<Ok<GroupGetByIdResponse>, ProblemHttpResult>> HandleAsync(
        [AsParameters] GroupGetByIdRequestParameters parameters
    )
    {
        var (groupId, handler, cancellationToken) = parameters;

        var getById = GroupGetById.Of(groupId);
        var result = await handler.Handle(getById, cancellationToken);

        return TypedResults.Ok(new GroupGetByIdResponse(result.Group));
    }
}

public sealed record GroupGetByIdRequestParameters(
    [FromRoute] Guid GroupId,
    GroupGetByIdHandler Handler,
    CancellationToken CancellationToken
);

// A separate response type from handler result ensures the API contract is decoupled from handler logic, providing flexibility to modify the handler's response
// or internal structures without breaking the externally exposed API format.
public sealed record GroupGetByIdResponse(GroupDto Group);
