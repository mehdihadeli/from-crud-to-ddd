using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartCharging.IntegrationTests.Groups.Mocks;
using SmartCharging.ServiceDefaults.Exceptions;
using SmartCharging.Shared.Data;
using SmartCharging.TestsShared.Fixtures;
using SmartChargingApi;
using SmartChargingApi.Groups;
using SmartChargingApi.Groups.Features.AddStationConnectors.v1;
using SmartChargingApi.Groups.Models;
using SmartChargingApi.Groups.Models.ValueObjects;
using SmartChargingApi.Shared.Data;

namespace SmartCharging.IntegrationTests.Groups.Features.AddStationConnectors.v1;

public class AddStationConnectorsTests(SharedFixture<SmartChargingMetadata, SmartChargingDbContext> sharedFixture)
    : SmartChargingIntegrationTestBase(sharedFixture)
{
    [Fact]
    internal async Task AddStationConnectors_WhenDataIsValid_AddsConnectorsSuccessfully()
    {
        // Arrange
        var fakeGroup = new GroupFake(
            numberOfConnectorsPerStation: 3,
            groupCapacity: 300,
            maxConnectorCurrent: 50
        ).Generate();
        await SharedFixture.ExecuteEfDbContextAsync(async db =>
        {
            await db.Groups.AddAsync(fakeGroup);
            await db.SaveChangesAsync();
        });

        var targetChargeStation = fakeGroup.ChargeStations.First();
        var newConnectors = new List<Connector>
        {
            Connector.Create(ConnectorId.Of(4), CurrentInAmps.Of(30)),
            Connector.Create(ConnectorId.Of(5), CurrentInAmps.Of(20)),
        };

        var addStationConnectors = SmartChargingApi.Groups.Features.AddStationConnectors.v1.AddStationConnectors.Of(
            fakeGroup.Id.Value,
            targetChargeStation.Id.Value,
            newConnectors.ToConnectorsDto()
        );

        var handler = Scope.ServiceProvider.GetRequiredService<AddConnectorsHandler>();

        // Act
        var result = await handler.Handle(addStationConnectors, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Connectors.Count.ShouldBe(2);

        var updatedGroup = await SharedFixture.ExecuteEfDbContextAsync(async db =>
        {
            return await db
                .Groups.Include(g => g.ChargeStations)
                .ThenInclude(cs => cs.Connectors)
                .FirstOrDefaultAsync(g => g.Id == fakeGroup.Id);
        });

        updatedGroup.ShouldNotBeNull();
        var updatedStation = updatedGroup.ChargeStations.First();
        updatedStation.Connectors.Count.ShouldBe(5);
        updatedStation.Connectors.Any(c => c.Id.Value == 4).ShouldBeTrue();
        updatedStation.Connectors.Any(c => c.Id.Value == 5).ShouldBeTrue();
    }

    [Fact]
    internal async Task AddStationConnectors_WhenGroupDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var nonExistentGroupId = Guid.NewGuid();
        var chargeStationId = Guid.NewGuid();
        var newConnectors = new List<Connector> { Connector.Create(ConnectorId.Of(1), CurrentInAmps.Of(30)) };

        var addStationConnectors = SmartChargingApi.Groups.Features.AddStationConnectors.v1.AddStationConnectors.Of(
            nonExistentGroupId,
            chargeStationId,
            newConnectors.ToConnectorsDto()
        );

        var handler = Scope.ServiceProvider.GetRequiredService<AddConnectorsHandler>();

        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(() =>
            handler.Handle(addStationConnectors, CancellationToken.None)
        );

        exception.Message.ShouldContain($"Group with ID {nonExistentGroupId} not found.");
    }

    [Fact]
    internal async Task AddStationConnectors_WhenChargeStationDoesNotExist_ThrowsDomainException()
    {
        // Arrange
        var fakeGroup = new GroupFake(numberOfConnectorsPerStation: 3).Generate();
        await SharedFixture.ExecuteEfDbContextAsync(async db =>
        {
            await db.Groups.AddAsync(fakeGroup);
            await db.SaveChangesAsync();
        });

        var nonExistentChargeStationId = Guid.NewGuid();
        var newConnectors = new List<Connector> { Connector.Create(ConnectorId.Of(1), CurrentInAmps.Of(30)) };

        var addStationConnectors = SmartChargingApi.Groups.Features.AddStationConnectors.v1.AddStationConnectors.Of(
            fakeGroup.Id.Value,
            nonExistentChargeStationId,
            newConnectors.ToConnectorsDto()
        );

        var handler = Scope.ServiceProvider.GetRequiredService<AddConnectorsHandler>();

        // Act & Assert
        var exception = await Should.ThrowAsync<DomainException>(() =>
            handler.Handle(addStationConnectors, CancellationToken.None)
        );

        exception.Message.ShouldBe("Charge station not found.");
    }

    [Fact]
    internal async Task AddStationConnectors_WhenConnectorIdsAreDuplicate_ThrowsDomainException()
    {
        // Arrange
        var fakeGroup = new GroupFake(numberOfConnectorsPerStation: 3).Generate();
        await SharedFixture.ExecuteEfDbContextAsync(async db =>
        {
            await db.Groups.AddAsync(fakeGroup);
            await db.SaveChangesAsync();
        });

        var targetChargeStation = fakeGroup.ChargeStations.First();
        var newConnectors = new List<Connector>
        {
            // add duplicate connector
            Connector.Create(ConnectorId.Of(3), CurrentInAmps.Of(40)),
        };

        var addStationConnectors = SmartChargingApi.Groups.Features.AddStationConnectors.v1.AddStationConnectors.Of(
            fakeGroup.Id.Value,
            targetChargeStation.Id.Value,
            newConnectors.ToConnectorsDto()
        );

        var handler = Scope.ServiceProvider.GetRequiredService<AddConnectorsHandler>();

        // Act & Assert
        var exception = await Should.ThrowAsync<DomainException>(() =>
            handler.Handle(addStationConnectors, CancellationToken.None)
        );

        exception.Message.ShouldBe("Connector IDs must be unique within a charge station.");
    }
}
