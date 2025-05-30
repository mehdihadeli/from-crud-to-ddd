using System.Net;
using System.Net.Http.Json;
using SmartCharging.EndToEndTests.Group.Mocks;
using SmartCharging.Groups.Features.GroupGetById.v1;
using SmartCharging.Shared.Application.Data;
using SmartCharging.TestsShared.Fixtures;
using Xunit.Abstractions;

namespace SmartCharging.EndToEndTests.Group.Features.GroupGetById.v1;

public class GroupGetByIdTests(
    SharedFixture<SmartChargingMetadata, SmartChargingDbContext> sharedFixture,
    ITestOutputHelper outputHelper
) : SmartChargingEndToEndTestBase(sharedFixture, outputHelper)
{
    [Fact]
    internal async Task GetGroupByIdEndpoint_WithValidId_Should_ReturnGroupDetails()
    {
        var fakeGroup = new GroupFake(numberOfConnectorsPerStation: 3).Generate();

        await SharedFixture.ExecuteEfDbContextAsync(async db =>
        {
            await db.Groups.AddAsync(fakeGroup);
            await db.SaveChangesAsync();
        });

        var groupId = fakeGroup.Id;
        var getGroupRoute = Constants.Routes.Groups.GetById(groupId.Value);

        var getGroupResponse = await SharedFixture.GuestClient.GetFromJsonAsync<GroupGetByIdResponse>(getGroupRoute);

        getGroupResponse.ShouldNotBeNull();
        getGroupResponse.Group.ShouldNotBeNull();

        getGroupResponse.Group.GroupId.ShouldBe(groupId.Value);
        getGroupResponse.Group.Name.ShouldBe(fakeGroup.Name.Value);
        getGroupResponse.Group.CapacityInAmps.ShouldBe(fakeGroup.CapacityInAmps.Value);

        getGroupResponse.Group.ChargeStations.Count.ShouldBe(1);
        var chargeStation = getGroupResponse.Group.ChargeStations.First();
        chargeStation.Name.ShouldBe(fakeGroup.ChargeStations.First().Name.Value);
        chargeStation.Connectors.Count.ShouldBe(fakeGroup.ChargeStations.First().Connectors.Count);
    }

    [Fact]
    internal async Task GetGroupByIdEndpoint_WithNonExistentId_Should_ReturnNotFound()
    {
        // Arrange
        var nonExistentGroupId = Guid.NewGuid();
        var getGroupRoute = Constants.Routes.Groups.GetById(nonExistentGroupId);

        // Act
        var getResponse = await SharedFixture.GuestClient.GetAsync(getGroupRoute);

        // Assert
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
