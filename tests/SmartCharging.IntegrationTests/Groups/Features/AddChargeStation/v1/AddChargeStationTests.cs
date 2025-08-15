using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartCharging.IntegrationTests.Groups.Mocks;
using SmartCharging.ServiceDefaults.Exceptions;
using SmartCharging.Shared.Data;
using SmartCharging.TestsShared.Fixtures;
using SmartChargingApi;
using SmartChargingApi.Groups;
using SmartChargingApi.Groups.Features.AddChargeStation.v1;
using SmartChargingApi.Shared.Data;

namespace SmartCharging.IntegrationTests.Groups.Features.AddChargeStation.v1;

public class AddChargeStationTests(SharedFixture<SmartChargingMetadata, SmartChargingDbContext> sharedFixture)
    : SmartChargingIntegrationTestBase(sharedFixture)
{
    [Fact]
    internal async Task AddChargeStation_WhenGroupExists_AddsChargeStationToGroupSuccessfully()
    {
        // Arrange
        var fakeGroup = new GroupFake(numberOfConnectorsPerStation: 3).Generate();

        // Create a group in the database so that it exists before we call AddChargeStation
        await SharedFixture.ExecuteEfDbContextAsync(async db =>
        {
            await db.Groups.AddAsync(fakeGroup);
            await db.SaveChangesAsync();
        });

        var newChargeStation = new ChargeStationFake(3).Generate();
        var addChargeStation = SmartChargingApi.Groups.Features.AddChargeStation.v1.AddChargeStation.Of(
            fakeGroup.Id.Value,
            newChargeStation.Name.Value,
            newChargeStation.Connectors.ToConnectorsDto()
        );

        var handler = Scope.ServiceProvider.GetRequiredService<AddChargeStationHandler>();

        // Act
        var chargeStationId = await handler.Handle(addChargeStation, CancellationToken.None);

        // Assert
        var updatedGroup = await SharedFixture.ExecuteEfDbContextAsync(async db =>
        {
            return await db
                .Groups.Include(g => g.ChargeStations)
                .ThenInclude(cs => cs.Connectors)
                .FirstOrDefaultAsync(g => g.Id == fakeGroup.Id);
        });

        updatedGroup.ShouldNotBeNull();
        var firstChargeStation = updatedGroup.ChargeStations.FirstOrDefault(x =>
            x.Id.Value == addChargeStation.ChargeStationId
        );
        firstChargeStation.ShouldNotBeNull();
        firstChargeStation.Id.Value.ShouldBe(chargeStationId);
        firstChargeStation.Connectors.Count.ShouldBe(3);
    }

    [Fact]
    internal async Task AddChargeStation_WhenGroupDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var nonExistentGroupId = Guid.NewGuid();
        var newChargeStation = new ChargeStationFake(3).Generate();

        var addChargeStation = SmartChargingApi.Groups.Features.AddChargeStation.v1.AddChargeStation.Of(
            nonExistentGroupId,
            newChargeStation.Name.Value,
            newChargeStation.Connectors.ToConnectorsDto()
        );

        var handler = Scope.ServiceProvider.GetRequiredService<AddChargeStationHandler>();

        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(() =>
            handler.Handle(addChargeStation, CancellationToken.None)
        );

        exception.Message.ShouldBe($"Group with ID {nonExistentGroupId} not found.");
    }
}
