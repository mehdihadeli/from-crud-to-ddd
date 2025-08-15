using Humanizer;
using SmartChargingStatisticsApi.GroupStatistics.Features.GroupCapacity;
using SmartChargingStatisticsApi.GroupStatistics.Features.GroupEnergy;

namespace SmartChargingStatisticsApi.GroupStatistics;

public static class GroupStatisticsConfig
{
    public static IEndpointRouteBuilder MapGroupStatisticsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // tags = "GroupStatistics"
        var groupStatistics = endpoints.NewVersionedApi(nameof(GroupStatistics).Pluralize());
        var groupStatisticsV1 = groupStatistics
            .MapGroup("/api/v{version:apiVersion}/" + nameof(GroupStatistics).Kebaberize())
            .HasApiVersion(1.0);

        groupStatisticsV1.MapGroupEnergyEndpoint();
        groupStatisticsV1.MapGroupCapacityEndpoint();

        return endpoints;
    }
}
