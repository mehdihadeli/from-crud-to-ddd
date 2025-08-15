using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using SmartCharging.EndToEndTests.Group.Mocks;
using SmartCharging.Shared.Data;
using SmartCharging.TestsShared.Fixtures;
using SmartChargingApi;
using SmartChargingApi.Groups.Features.AddChargeStation.v1;
using SmartChargingApi.Shared.Data;

namespace SmartCharging.EndToEndTests.Group.Features.AddChargeStation.v1;

//TestName: `MethodName_Condition_ExpectedResult`

public class AddChargeStationTests(SharedFixture<SmartChargingMetadata, SmartChargingDbContext> sharedFixture)
    : SmartChargingEndToEndTestBase(sharedFixture)
{
    [Fact]
    internal async Task AddChargeStation_WhenGroupExistsAndRequestIsValid_AddsChargeStationSuccessfully()
    {
        // Arrange
        var fakeGroup = new GroupFake(numberOfConnectorsPerStation: 1).Generate();
        await SharedFixture.ExecuteEfDbContextAsync(async db =>
        {
            await db.Groups.AddAsync(fakeGroup);
            await db.SaveChangesAsync();
        });

        var groupId = fakeGroup.Id.Value;
        var addChargeStationRoute = Constants.Routes.Groups.AddChargeStation(groupId);

        var request = new AddChargeStationRequest(
            Name: "Test Charge Station",
            ConnectorsRequest: new List<AddChargeStationRequest.CreateConnectorRequest> { new(2, 32), new(3, 40) }
        );

        // Act
        var addResponse = await SharedFixture.GuestClient.PostAsJsonAsync(
            addChargeStationRoute,
            request,
            cancellationToken: TestContext.Current.CancellationToken
        );
        addResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var responseContent = await addResponse.Content.ReadFromJsonAsync<AddChargeStationResponse>(
            cancellationToken: TestContext.Current.CancellationToken
        );
        responseContent.ShouldNotBeNull();
        responseContent.ChargeStationId.ShouldNotBe(Guid.Empty);

        // Verify the charge station exists in the database
        var updatedGroup = await SharedFixture.ExecuteEfDbContextAsync(async db =>
        {
            return await db
                .Groups.Include(g => g.ChargeStations)
                .ThenInclude(cs => cs.Connectors)
                .FirstOrDefaultAsync(g => g.Id == fakeGroup.Id);
        });

        updatedGroup.ShouldNotBeNull();
        updatedGroup.ChargeStations.Count.ShouldBe(2);

        var chargeStation = updatedGroup.ChargeStations.FirstOrDefault(cs =>
            cs.Id.Value == responseContent.ChargeStationId
        );
        chargeStation.ShouldNotBeNull();
        chargeStation.Name.Value.ShouldBe(request.Name);
        chargeStation.Connectors.Count.ShouldBe(2);
        chargeStation.Connectors.Select(c => c.Id.Value).ShouldBe(new[] { 2, 3 });
    }

    [Fact]
    internal async Task AddChargeStation_WhenGroupDoesNotExist_ReturnsNotFound()
    {
        // Arrange - Use a non-existent group ID
        var nonExistentGroupId = Guid.NewGuid();
        var addChargeStationRoute = Constants.Routes.Groups.AddChargeStation(nonExistentGroupId);

        var request = new AddChargeStationRequest(
            Name: "Test Charge Station",
            ConnectorsRequest: new List<AddChargeStationRequest.CreateConnectorRequest> { new(1, 32), new(2, 40) }
        );

        // Act - Attempt to add a charge station to a non-existent group
        var addResponse = await SharedFixture.GuestClient.PostAsJsonAsync(
            addChargeStationRoute,
            request,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert - Validate the response is 404 Not Found
        addResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
