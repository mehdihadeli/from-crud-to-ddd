using System.Net;
using SmartCharging.EndToEndTests.Group.Mocks;
using SmartCharging.ServiceDefaults.Extensions;
using SmartCharging.TestsShared.Fixtures;
using SmartChargingApi;
using SmartChargingApi.Groups.Features.GetGroupById.v1;
using SmartChargingApi.Shared.Data;

namespace SmartCharging.EndToEndTests.Group.Features.GroupGetById.v1;

public class GetGroupByIdTests(SharedFixture<SmartChargingMetadata, SmartChargingDbContext> sharedFixture)
    : SmartChargingEndToEndTestBase(sharedFixture)
{
    [Fact]
    internal async Task GetGroupById_WhenGroupExistsAndBothStatsPresent_ReturnsGroupWithAllStatistics()
    {
        var fakeGroup = new GroupFake(3).Generate();
        await SharedFixture.ExecuteEfDbContextAsync(async db =>
        {
            await db.Groups.AddAsync(fakeGroup);
            await db.SaveChangesAsync();
        });

        var groupId = fakeGroup.Id;
        var getGroupRoute = Constants.Routes.Groups.GetById(groupId.Value);
        var (capacityStats, _) = GroupStatisticsExternalServiceMock.SetupGetCapacityStatistics(groupId.Value);
        var (energyStats, _) = GroupStatisticsExternalServiceMock.SetupGetEnergyStatistics(groupId.Value);

        var response = await SharedFixture.GuestClient.GetAsync(getGroupRoute, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var group = await response.ReadResponseContentAsync<GetGroupByIdResponse>(
            TestContext.Current.CancellationToken
        );

        group.ShouldNotBeNull();
        group.CapacityStats.ShouldNotBeNull();
        group.EnergyStats.ShouldNotBeNull();
        group.CapacityStats!.MaxCapacityAmps.ShouldBe(capacityStats.MaxCapacityAmps);
        group.EnergyStats!.EnergyUsedKWh.ShouldBe(energyStats.EnergyUsedKWh);
    }

    [Fact]
    internal async Task GetGroupById_WhenGroupExistsAndBothStatsNoContent_ReturnsGroupWithNullStatistics()
    {
        var fakeGroup = new GroupFake(2).Generate();
        await SharedFixture.ExecuteEfDbContextAsync(async db =>
        {
            await db.Groups.AddAsync(fakeGroup);
            await db.SaveChangesAsync();
        });

        var groupId = fakeGroup.Id;
        var getGroupRoute = Constants.Routes.Groups.GetById(groupId.Value);
        GroupStatisticsExternalServiceMock.SetupNoContentCapacityStatistics(groupId.Value);
        GroupStatisticsExternalServiceMock.SetupNoContentEnergyStatistics(groupId.Value);

        var response = await SharedFixture.GuestClient.GetAsync(getGroupRoute, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var group = await response.ReadResponseContentAsync<GetGroupByIdResponse>(
            TestContext.Current.CancellationToken
        );
        group.ShouldNotBeNull();
        group.CapacityStats.ShouldBeNull();
        group.EnergyStats.ShouldBeNull();
    }

    [Fact]
    internal async Task GetGroupById_WhenGroupExistsAndBothStatsNotFound_ReturnsGroupWithNullStatistics()
    {
        var fakeGroup = new GroupFake(1).Generate();
        await SharedFixture.ExecuteEfDbContextAsync(async db =>
        {
            await db.Groups.AddAsync(fakeGroup);
            await db.SaveChangesAsync();
        });

        var groupId = fakeGroup.Id;
        var getGroupRoute = Constants.Routes.Groups.GetById(groupId.Value);
        GroupStatisticsExternalServiceMock.SetupNotFoundCapacityStatistics(groupId.Value);
        GroupStatisticsExternalServiceMock.SetupNotFoundEnergyStatistics(groupId.Value);

        var response = await SharedFixture.GuestClient.GetAsync(getGroupRoute, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var group = await response.ReadResponseContentAsync<GetGroupByIdResponse>(
            TestContext.Current.CancellationToken
        );
        group.ShouldNotBeNull();
        group.CapacityStats.ShouldBeNull();
        group.EnergyStats.ShouldBeNull();
    }

    [Fact]
    internal async Task GetGroupById_WhenGroupExistsAndOnlyCapacityStatsPresent_ReturnsGroupWithOnlyCapacityStatistics()
    {
        var fakeGroup = new GroupFake(3).Generate();
        await SharedFixture.ExecuteEfDbContextAsync(async db =>
        {
            await db.Groups.AddAsync(fakeGroup);
            await db.SaveChangesAsync();
        });

        var groupId = fakeGroup.Id;
        var getGroupRoute = Constants.Routes.Groups.GetById(groupId.Value);
        var (capacityStats, _) = GroupStatisticsExternalServiceMock.SetupGetCapacityStatistics(groupId.Value);
        GroupStatisticsExternalServiceMock.SetupNoContentEnergyStatistics(groupId.Value);

        var response = await SharedFixture.GuestClient.GetAsync(getGroupRoute, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var group = await response.ReadResponseContentAsync<GetGroupByIdResponse>(
            TestContext.Current.CancellationToken
        );
        group.ShouldNotBeNull();
        group.CapacityStats.ShouldNotBeNull();
        group.CapacityStats!.MaxCapacityAmps.ShouldBe(capacityStats.MaxCapacityAmps);
        group.EnergyStats.ShouldBeNull();
    }

    [Fact]
    internal async Task GetGroupById_WhenGroupExistsAndOnlyEnergyStatsPresent_ReturnsGroupWithOnlyEnergyStatistics()
    {
        var fakeGroup = new GroupFake(2).Generate();
        await SharedFixture.ExecuteEfDbContextAsync(async db =>
        {
            await db.Groups.AddAsync(fakeGroup);
            await db.SaveChangesAsync();
        });

        var groupId = fakeGroup.Id;
        var getGroupRoute = Constants.Routes.Groups.GetById(groupId.Value);
        GroupStatisticsExternalServiceMock.SetupNoContentCapacityStatistics(groupId.Value);
        var (energyStats, _) = GroupStatisticsExternalServiceMock.SetupGetEnergyStatistics(groupId.Value);

        var response = await SharedFixture.GuestClient.GetAsync(getGroupRoute, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var group = await response.ReadResponseContentAsync<GetGroupByIdResponse>(
            TestContext.Current.CancellationToken
        );
        group.ShouldNotBeNull();
        group.CapacityStats.ShouldBeNull();
        group.EnergyStats.ShouldNotBeNull();
        group.EnergyStats!.EnergyUsedKWh.ShouldBe(energyStats.EnergyUsedKWh);
    }

    [Fact]
    internal async Task GetGroupById_WhenGroupDoesNotExist_ReturnsNotFound()
    {
        var nonExistentGroupId = Guid.NewGuid();
        var getGroupRoute = Constants.Routes.Groups.GetById(nonExistentGroupId);

        var response = await SharedFixture.GuestClient.GetAsync(getGroupRoute, TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
