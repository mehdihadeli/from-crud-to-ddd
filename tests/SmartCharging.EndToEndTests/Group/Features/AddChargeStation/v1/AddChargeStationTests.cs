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
            ChargeStationId: newChargeStationId,
            Name: "Test Charge Station",
            Connectors: new List<ConnectorDto>
            {
                new ConnectorDto(newChargeStationId, 2, 32),
                new ConnectorDto(newChargeStationId, 3, 40),
            }
        );

        // Act
        var addResponse = await SharedFixture.GuestClient.PostAsJsonAsync(addChargeStationRoute, request);

        // Assert
        addResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        // Verify the charge station exists in the database
        var updatedGroup = await SharedFixture.ExecuteEfDbContextAsync(async db =>
        {
            return await db
                .Groups.Include(g => g.ChargeStations)
                .ThenInclude(cs => cs.Connectors)
                .FirstOrDefaultAsync(g => g.Id == fakeGroup.Id);
        });

        updatedGroup.ShouldNotBeNull();
        // one station exist in the group, and the other is added via the AddChargeStation endpoint
        updatedGroup.ChargeStations.Count.ShouldBe(2);
        var chargeStation = updatedGroup.ChargeStations.Last();
        chargeStation.Id.Value.ShouldBe(request.ChargeStationId.Value);
        chargeStation.Name.Value.ShouldBe(request.Name);
        // connectors for the new added charge station which is 2
        chargeStation.Connectors.Count.ShouldBe(2);
    }

    [Fact]
    internal async Task AddChargeStation_GroupDoesNotExist_Should_ReturnNotFound()
    {
        // Arrange - Use a non-existent group ID
        var nonExistentGroupId = Guid.NewGuid();
        var addChargeStationRoute = Constants.Routes.Groups.AddChargeStation(nonExistentGroupId);

        var newChargeStationId = Guid.NewGuid();
        var request = new AddChargeStationRequest(
            ChargeStationId: newChargeStationId,
            Name: "Test Charge Station",
            Connectors: new List<ConnectorDto>
            {
                new ConnectorDto(newChargeStationId, 1, 32),
                new ConnectorDto(newChargeStationId, 2, 40),
            }
        );

        // Act - Attempt to add a charge station to a non-existent group
        var addResponse = await SharedFixture.GuestClient.PostAsJsonAsync(addChargeStationRoute, request);

        // Assert - Validate the response is 404 Not Found
        addResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    internal async Task AddChargeStation_WithDuplicateId_Should_ReturnConflict()
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

        var duplicateStationId = fakeGroup.ChargeStations.First().Id.Value;

        var request = new AddChargeStationRequest(
            // Duplicate ID
            ChargeStationId: duplicateStationId,
            Name: "Duplicate Charge Station",
            Connectors: new List<ConnectorDto>
            {
                new ConnectorDto(duplicateStationId, 1, 32),
                new ConnectorDto(duplicateStationId, 2, 40),
            }
        );

        // Act - Attempt to add a charge station with an existing ID
        var addResponse = await SharedFixture.GuestClient.PostAsJsonAsync(addChargeStationRoute, request);

        // Assert - Validate the response is 409 Conflict
        addResponse.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }
}
