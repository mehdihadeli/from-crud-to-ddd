using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartCharging.Groups.Dtos;

namespace SmartCharging.Groups.Features.GetGroupsByPage.v1;

public static class GetGroupsByPageEndpoint
{
    public static RouteHandlerBuilder MapGetGroupsByPageEndpoint(this RouteGroupBuilder group)
    {
        return group
            .MapGet("/", HandleAsync)
            .WithName(nameof(GetGroupsByPage))
            .WithDisplayName(nameof(GetGroupsByPage).Humanize())
            .WithSummary("Retrieves Groups by page.")
            .WithDescription(
                "Fetches a paginated list of Groups. The result includes basic details about Groups such as ID, Name, Capacity, and the number of Charge Stations in each Group."
            )
            .Produces<GetGroupsByPageResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem();
    }

    static async Task<Ok<GetGroupsByPageResponse>> HandleAsync(
        [AsParameters] GetGroupsByPageRequestParameters parameters
    )
    {
        var (handler, cancellationToken, pageNumber, pageSize) = parameters;

        var getGroupsByPage = new GetGroupsByPage(pageNumber, pageSize);
        var result = await handler.Handle(getGroupsByPage, cancellationToken);

        return TypedResults.Ok(
            new GetGroupsByPageResponse(result.Groups, result.PageSize, result.PageCount, result.TotalCount)
        );
    }
}

public sealed record GetGroupsByPageRequestParameters(
    GetGroupsByPageHandler Handler,
    CancellationToken CancellationToken,
    [FromQuery] int PageNumber = 1,
    [FromQuery] int PageSize = 5
);

// A separate response type from handler result ensures the API contract is decoupled from handler logic, providing flexibility to modify the handler's response
// or internal structures without breaking the externally exposed API format.
public sealed record GetGroupsByPageResponse(
    IReadOnlyCollection<GroupDto> Groups,
    int PageSize,
    int PageCount,
    int TotalCount
);
