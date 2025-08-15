using Bogus;
using Humanizer;
using SmartChargingStatisticsApi.GroupStatistics.Dtos;

namespace SmartChargingStatisticsApi.GroupStatistics.Features.GroupCapacity;

public static class GroupCapacityEndpoint
{
    public static RouteHandlerBuilder MapGroupCapacityEndpoint(this IEndpointRouteBuilder app)
    {
        return app.MapGet(
                "/group-capacity/{groupId:guid}",
                (Guid groupId) =>
                {
                    var capacityFaker = new Faker<GroupCapacityStatisticsClientResponse>()
                        .UseSeed(123)
                        .CustomInstantiator(f =>
                        {
                            var maxCapacity = f.PickRandom(100, 150, 200, 250);
                            var currentLoad = f.Random.Int(0, maxCapacity);
                            var available = Math.Max(maxCapacity - currentLoad, 0);
                            return new GroupCapacityStatisticsClientResponse(
                                groupId,
                                currentLoad,
                                maxCapacity,
                                available
                            );
                        });

                    // Generate fake realistic data but keep groupId fixed for a result
                    var stats = capacityFaker.Generate();
                    stats = stats with { GroupId = groupId };

                    return Results.Ok(stats);
                }
            )
            .WithName(nameof(GroupCapacity))
            .WithDisplayName(nameof(GroupCapacity).Humanize())
            .WithSummary("")
            .WithDescription("")
            .Produces(StatusCodes.Status200OK);
    }
}
