using SmartChargingStatisticsApi.GroupStatistics;

namespace SmartChargingStatisticsApi.Shared;

public static class ApplicationConfig
{
    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        return builder;
    }

    public static IEndpointRouteBuilder MapApplicationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGroupStatisticsEndpoints();

        return endpoints;
    }
}
