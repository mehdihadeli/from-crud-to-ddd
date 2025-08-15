using Bogus;
using SmartChargingStatisticsApi.GroupStatistics.Dtos;

namespace SmartChargingStatisticsApi.GroupStatistics.Features.GroupEnergy;

public static class GroupEnergyEndpoint
{
    public static RouteHandlerBuilder MapGroupEnergyEndpoint(this IEndpointRouteBuilder app)
    {
        return app.MapGet(
            "/group-energy/{groupId:guid}",
            (Guid groupId) =>
            {
                var energyFaker = new Faker<GroupEnergyConsumptionClientResponse>()
                    .UseSeed(456)
                    .CustomInstantiator(f =>
                    {
                        var energyUsed = Math.Round(f.Random.Double(0, 5000), 2);
                        var periodEnd = DateTime.UtcNow;
                        var periodStart = periodEnd.AddDays(-f.Random.Int(7, 30));
                        return new GroupEnergyConsumptionClientResponse(groupId, energyUsed, periodStart, periodEnd);
                    });

                var stats = energyFaker.Generate();
                stats = stats with { GroupId = groupId };

                return Results.Ok(stats);
            }
        );
    }
}
