using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using SmartCharging.EndToEndTests.Group.Mocks;
using SmartCharging.Groups.Dtos;
using SmartCharging.Groups.Features.AddChargeStation.v1;
using SmartCharging.Shared.Application.Data;
using SmartCharging.TestsShared.Fixtures;
using Xunit.Abstractions;

namespace SmartCharging.EndToEndTests.Group.Features.AddChargeStation.v1;

public class AddChargeStationTests(
    SharedFixture<SmartChargingMetadata, SmartChargingDbContext> sharedFixture,
    ITestOutputHelper outputHelper
) : SmartChargingEndToEndTestBase(sharedFixture, outputHelper)
{
    [Fact]
    internal async Task AddChargeStation_WithValidData_Should_AddChargeStationSuccessfully()
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

        var newChargeStationId = Guid.NewGuid();
        var request = new AddChargeStationRequest(
            Name: "Test Charge Station",
            Connectors: new List<ConnectorDto>
            {
                new ConnectorDto(newChargeStationId, 2, 32),
                new ConnectorDto(newChargeStationId, 3, 40),
            }
        );

        // Act
        var addResponse = await SharedFixture.GuestClient.PostAsJsonAsync(addChargeStationRoute, request);
        addResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var responseContent = await addResponse.Content.ReadFromJsonAsync<AddChargeStationResponse>();
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
    internal async Task AddChargeStation_GroupDoesNotExist_Should_ReturnNotFound()
    {
        // Arrange - Use a non-existent group ID
        var nonExistentGroupId = Guid.NewGuid();
        var addChargeStationRoute = Constants.Routes.Groups.AddChargeStation(nonExistentGroupId);

        var request = new AddChargeStationRequest(
            Name: "Test Charge Station",
            Connectors: new List<ConnectorDto> { new(nonExistentGroupId, 1, 32), new(nonExistentGroupId, 2, 40) }
        );

        // Act - Attempt to add a charge station to a non-existent group
        var addResponse = await SharedFixture.GuestClient.PostAsJsonAsync(addChargeStationRoute, request);

        // Assert - Validate the response is 404 Not Found
        addResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
