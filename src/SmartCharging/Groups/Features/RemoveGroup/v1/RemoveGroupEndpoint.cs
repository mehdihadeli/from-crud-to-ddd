using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace SmartCharging.Groups.Features.RemoveGroup.v1;

public static class RemoveGroupEndpoint
{
    public static RouteHandlerBuilder MapRemoveGroupEndpoint(this IEndpointRouteBuilder app)
    {
        return app.MapDelete("/{id:guid}", HandleAsync)
            .WithName(nameof(Features.RemoveGroup))
            .WithDisplayName(nameof(Features.RemoveGroup).Humanize())
            .WithSummary("Removes a group along with its associated charge stations and connectors.")
            .WithDescription("Removes a group along with its associated charge stations and connectors.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();
    }

    static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [AsParameters] RemoveGroupRequestParameters parameters
    )
    {
        var (id, handler, cancellationToken) = parameters;

        var removeGroup = RemoveGroup.Of(id);
        await handler.Handle(removeGroup, cancellationToken);

        return TypedResults.NoContent();
    }
}

public sealed record RemoveGroupRequestParameters(
    [FromRoute] Guid Id,
    RemoveGroupHandler Handler,
    CancellationToken CancellationToken
);
