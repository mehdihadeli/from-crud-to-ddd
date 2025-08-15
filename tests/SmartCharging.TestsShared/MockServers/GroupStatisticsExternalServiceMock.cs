using System.Net;
using Bogus;
using SmartChargingApi.Groups.Dtos.Clients;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace SmartCharging.TestsShared.MockServers;

public class GroupStatisticsExternalServiceMock(WireMockServer wireMockServer)
{
    private const string CapacityStatsPath = "/api/v1/group-statistics/group-capacity";
    private const string EnergyStatsPath = "/api/v1/group-statistics/group-energy";

    public (GroupCapacityStatisticsClientResponseDto Response, string Endpoint) SetupGetCapacityStatistics(Guid groupId)
    {
        var faker = new Faker();
        var responseDto = new GroupCapacityStatisticsClientResponseDto(
            GroupId: groupId,
            CurrentLoadAmps: faker.Random.Int(10, 150),
            MaxCapacityAmps: faker.Random.Int(10, 150),
            AvailableCapacityAmps: faker.Random.Int(10, 150)
        );

        var url = $"{CapacityStatsPath}/{groupId}";
        wireMockServer
            .Given(Request.Create().UsingGet().WithPath(url))
            .RespondWith(Response.Create().WithBodyAsJson(responseDto).WithStatusCode(HttpStatusCode.OK));
        return (responseDto, url);
    }

    public (GroupCapacityStatisticsClientResponseDto? Response, string Endpoint) SetupNoContentCapacityStatistics(
        Guid groupId
    )
    {
        var url = $"{CapacityStatsPath}/{groupId}";
        wireMockServer
            .Given(Request.Create().UsingGet().WithPath(url))
            .RespondWith(Response.Create().WithStatusCode(HttpStatusCode.NoContent));
        return (null, url);
    }

    public (GroupCapacityStatisticsClientResponseDto? Response, string Endpoint) SetupNotFoundCapacityStatistics(
        Guid groupId
    )
    {
        var url = $"{CapacityStatsPath}/{groupId}";
        wireMockServer
            .Given(Request.Create().UsingGet().WithPath(url))
            .RespondWith(Response.Create().WithStatusCode(HttpStatusCode.NotFound));
        return (null, url);
    }

    public (GroupEnergyConsumptionClientResponseDto Response, string Endpoint) SetupGetEnergyStatistics(Guid groupId)
    {
        var faker = new Faker();
        var responseDto = new GroupEnergyConsumptionClientResponseDto(
            groupId,
            EnergyUsedKWh: faker.Random.Int(1000, 5000),
            PeriodStart: DateTime.UtcNow.AddDays(-7),
            PeriodEnd: DateTime.UtcNow
        );

        var url = $"{EnergyStatsPath}/{groupId}";
        wireMockServer
            .Given(Request.Create().UsingGet().WithPath(url))
            .RespondWith(Response.Create().WithBodyAsJson(responseDto).WithStatusCode(HttpStatusCode.OK));
        return (responseDto, url);
    }

    public (GroupEnergyConsumptionClientResponseDto? Response, string Endpoint) SetupNoContentEnergyStatistics(
        Guid groupId
    )
    {
        var url = $"{EnergyStatsPath}/{groupId}";
        wireMockServer
            .Given(Request.Create().UsingGet().WithPath(url))
            .RespondWith(Response.Create().WithStatusCode(HttpStatusCode.NoContent));
        return (null, url);
    }

    public (GroupEnergyConsumptionClientResponseDto? Response, string Endpoint) SetupNotFoundEnergyStatistics(
        Guid groupId
    )
    {
        var url = $"{EnergyStatsPath}/{groupId}";
        wireMockServer
            .Given(Request.Create().UsingGet().WithPath(url))
            .RespondWith(Response.Create().WithStatusCode(HttpStatusCode.NotFound));
        return (null, url);
    }
}
