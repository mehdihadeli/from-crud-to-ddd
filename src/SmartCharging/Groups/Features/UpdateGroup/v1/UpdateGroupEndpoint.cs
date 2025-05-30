using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace SmartCharging.Groups.Features.UpdateGroup.v1;

public static class UpdateGroupEndpoint
{
    public static RouteHandlerBuilder MapUpdateGroupEndpoint(this IEndpointRouteBuilder app)
    {
        return app.MapPut("/{id:guid}", Handle)
            .WithName(nameof(UpdateGroup))
            .WithDisplayName(nameof(UpdateGroup).Humanize())
            .WithSummary("Updates an existing Group's details, including its name and capacity.")
            .WithDescription(
                "Allows updating the details of an existing Group. The request must provide the Group ID along with the updated name and/or capacity. Validation ensures that the new capacity does not violate the constraints of dependent Charge Stations and Connectors."
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();

        static async Task<Results<NoContent, ValidationProblem, ProblemHttpResult>> Handle(
            [AsParameters] UpdateGroupRequestParameters parameters
        )
        {
            var (id, request, handler, cancellationToken) = parameters;

            var updateGroup = UpdateGroup.Of(id, request.Name, request.CapacityInAmps);

            await handler.Handle(updateGroup, cancellationToken);

            return TypedResults.NoContent();
        }
    }
}

public record UpdateGroupRequestParameters(
    [FromRoute] Guid Id,
    [FromBody] UpdateGroupRequest Request,
    UpdateGroupHandler Handler,
    CancellationToken CancellationToken
);

public record UpdateGroupRequest(string? Name, int CapacityInAmps);
