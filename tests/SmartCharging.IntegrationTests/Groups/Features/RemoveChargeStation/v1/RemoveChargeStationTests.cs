using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartCharging.IntegrationTests.Groups.Mocks;
using SmartCharging.ServiceDefaults.Exceptions;
using SmartCharging.Shared.Data;
using SmartCharging.TestsShared.Fixtures;
using SmartChargingApi;
using SmartChargingApi.Groups.Features.RemoveChargeStation.v1;
using SmartChargingApi.Shared.Data;

namespace SmartCharging.IntegrationTests.Groups.Features.RemoveChargeStation.v1;

public class RemoveChargeStationTests(SharedFixture<SmartChargingMetadata, SmartChargingDbContext> sharedFixture)
    : SmartChargingIntegrationTestBase(sharedFixture)
{
    [Fact]
    internal async Task RemoveChargeStation_WhenInputsAreValid_ShouldRemoveChargeStationSuccessfully()
    {
        // Arrange
        var fakeGroup = new GroupFake(numberOfConnectorsPerStation: 3).Generate();
        await SharedFixture.ExecuteEfDbContextAsync(async db =>
        {
            await db.Groups.AddAsync(fakeGroup);
            await db.SaveChangesAsync();
        });

        // Get the existing charge station
        var targetChargeStation = fakeGroup.ChargeStations.First();
        var removeChargeStation = SmartChargingApi.Groups.Features.RemoveChargeStation.v1.RemoveChargeStation.Of(
            fakeGroup.Id.Value,
            targetChargeStation.Id.Value
        );

        var handler = Scope.ServiceProvider.GetRequiredService<RemoveChargeStationHandler>();

        // Act
        await handler.Handle(removeChargeStation, CancellationToken.None);

        // Assert
        await SharedFixture.ExecuteEfDbContextAsync(async db =>
        {
            var updatedGroup = await db
                .Groups.Include(g => g.ChargeStations)
                .ThenInclude(cs => cs.Connectors)
                .FirstOrDefaultAsync(g => g.Id == fakeGroup.Id);

            // check that the group aggregate has no charge stations
            updatedGroup.ShouldNotBeNull();
            updatedGroup.ChargeStations.ShouldBeEmpty();

            // check db for the removed station and connectors
            var existingStationOnDb = db.ChargeStations.FirstOrDefault(x => x.GroupId == fakeGroup.Id);
            existingStationOnDb.ShouldBeNull();

            var existingConnectorsOnDb = db.Connectors.Where(x => x.ChargeStationId == targetChargeStation.Id).ToList();
            existingConnectorsOnDb.ShouldBeEmpty();
        });
    }

    [Fact]
    internal async Task RemoveChargeStation_WhenGroupDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentGroupId = Guid.NewGuid();
        var chargeStationId = Guid.NewGuid();

        var removeChargeStation = SmartChargingApi.Groups.Features.RemoveChargeStation.v1.RemoveChargeStation.Of(
            nonExistentGroupId,
            chargeStationId
        );

        var handler = Scope.ServiceProvider.GetRequiredService<RemoveChargeStationHandler>();

        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(() =>
            handler.Handle(removeChargeStation, CancellationToken.None)
        );

        exception.Message.ShouldBe($"Group with ID {nonExistentGroupId} not found.");
    }

    [Fact]
    internal async Task RemoveChargeStation_WhenChargeStationDoesNotExistInGroup_ShouldThrowDomainException()
    {
        // Arrange
        var fakeGroup = new GroupFake(numberOfConnectorsPerStation: 3).Generate();
        await SharedFixture.ExecuteEfDbContextAsync(async db =>
        {
            await db.Groups.AddAsync(fakeGroup);
            await db.SaveChangesAsync();
        });

        var nonExistentChargeStationId = Guid.NewGuid();

        var removeChargeStation = SmartChargingApi.Groups.Features.RemoveChargeStation.v1.RemoveChargeStation.Of(
            fakeGroup.Id.Value,
            nonExistentChargeStationId
        );

        var handler = Scope.ServiceProvider.GetRequiredService<RemoveChargeStationHandler>();

        // Act & Assert
        var exception = await Should.ThrowAsync<DomainException>(() =>
            handler.Handle(removeChargeStation, CancellationToken.None)
        );

        exception.Message.ShouldBe("Charge station not found in this group");
    }

    [Fact]
    internal async Task RemoveChargeStation_WhenInputsAreNull_ShouldThrowValidationException()
    {
        // Act & Assert
        var handler = Scope.ServiceProvider.GetRequiredService<RemoveChargeStationHandler>();

        var exception = await Should.ThrowAsync<ValidationException>(() =>
            handler.Handle(null!, CancellationToken.None)
        );

        exception.Message.ShouldBe("removeChargeStation cannot be null or empty.");
    }

    [Fact]
    internal async Task RemoveChargeStation_WhenGroupHasMultipleStations_ShouldRemoveCorrectStation()
    {
        // Arrange
        var fakeGroup = new GroupFake(numberOfConnectorsPerStation: 3).Generate();
        var additionalChargeStation = new ChargeStationFake(3).Generate();
        fakeGroup.AddChargeStation(additionalChargeStation);

        await SharedFixture.ExecuteEfDbContextAsync(async db =>
        {
            await db.Groups.AddAsync(fakeGroup);
            await db.SaveChangesAsync();
        });

        var removeChargeStation = SmartChargingApi.Groups.Features.RemoveChargeStation.v1.RemoveChargeStation.Of(
            fakeGroup.Id.Value,
            additionalChargeStation.Id.Value
        );

        var handler = Scope.ServiceProvider.GetRequiredService<RemoveChargeStationHandler>();

        // Act
        await handler.Handle(removeChargeStation, CancellationToken.None);

        // Assert
        await SharedFixture.ExecuteEfDbContextAsync(async db =>
        {
            var updatedGroup = await db
                .Groups.Include(g => g.ChargeStations)
                .ThenInclude(cs => cs.Connectors)
                .FirstOrDefaultAsync(g => g.Id == fakeGroup.Id);

            updatedGroup.ShouldNotBeNull();
            updatedGroup.ChargeStations.Count.ShouldBe(1);

            var remainingStation = updatedGroup.ChargeStations.Single();
            remainingStation.Id.ShouldBe(fakeGroup.ChargeStations.First().Id);

            // check db for the removed station and connectors
            var existingStationsOnDb = db.ChargeStations.Where(x => x.GroupId == fakeGroup.Id);
            existingStationsOnDb.AsEnumerable().ShouldNotBeEmpty();
            existingStationsOnDb.Count().ShouldBe(1);
            existingStationsOnDb.FirstOrDefault(x => x.Id == additionalChargeStation.Id).ShouldBeNull();

            var existingConnectorsOnDb = db
                .Connectors.Where(x => x.ChargeStationId == additionalChargeStation.Id)
                .ToList();
            existingConnectorsOnDb.ShouldBeEmpty();
        });
    }
}
