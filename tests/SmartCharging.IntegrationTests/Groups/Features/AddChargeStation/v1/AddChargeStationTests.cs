using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartCharging.Groups;
using SmartCharging.Groups.Features.AddChargeStation.v1;
using SmartCharging.IntegrationTests.Groups.Mocks;
using SmartCharging.Shared.Application.Data;
using SmartCharging.Shared.BuildingBlocks.Exceptions;
using SmartCharging.TestsShared.Fixtures;
using Xunit.Abstractions;

namespace SmartCharging.IntegrationTests.Groups.Features.AddChargeStation.v1;

public class AddChargeStationTests(
    SharedFixture<SmartChargingMetadata, SmartChargingDbContext> sharedFixture,
    ITestOutputHelper outputHelper
) : SmartChargingIntegrationTestBase(sharedFixture, outputHelper)
{
    [Fact]
    internal async Task AddChargeStation_WithValidData_Should_AddChargeStationToGroupSuccessfully()
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
        var addChargeStation = SmartCharging.Groups.Features.AddChargeStation.v1.AddChargeStation.Of(
            fakeGroup.Id.Value,
            newChargeStation.Id.Value,
            newChargeStation.Name.Value,
            newChargeStation.Connectors.ToConnectorsDto()
        );

        var handler = Scope.ServiceProvider.GetRequiredService<AddChargeStationHandler>();

        // Act
        await handler.Handle(addChargeStation, CancellationToken.None);

        // Assert
        var updatedGroup = await SharedFixture.ExecuteEfDbContextAsync(async db =>
        {
            return await db
                .Groups.Include(g => g.ChargeStations)
                .ThenInclude(cs => cs.Connectors)
                .FirstOrDefaultAsync(g => g.Id == fakeGroup.Id);
        });

        updatedGroup.ShouldNotBeNull();
        updatedGroup.ChargeStations.Count.ShouldBe(2);
        updatedGroup.ChargeStations.Last().Id.ShouldBe(newChargeStation.Id);
        updatedGroup.ChargeStations.Last().Connectors.Count.ShouldBe(3);
    }

    [Fact]
    internal async Task AddChargeStation_ToNonExistentGroup_Should_ThrowNotFoundException()
    {
        // Arrange
        var nonExistentGroupId = Guid.NewGuid();
        var newChargeStation = new ChargeStationFake(3).Generate();

        var addChargeStation = SmartCharging.Groups.Features.AddChargeStation.v1.AddChargeStation.Of(
            nonExistentGroupId,
            newChargeStation.Id.Value,
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

    [Fact]
    internal async Task AddChargeStation_WithDuplicateId_Should_ThrowDomainException()
    {
        // Arrange
        var fakeGroup = new GroupFake(numberOfConnectorsPerStation: 3).Generate();

        await SharedFixture.ExecuteEfDbContextAsync(async db =>
        {
            await db.Groups.AddAsync(fakeGroup);
            await db.SaveChangesAsync();
        });

        // Use the same ChargeStation ID as an existing station within the group
        var duplicateChargeStation = fakeGroup.ChargeStations.First();

        var addChargeStation = SmartCharging.Groups.Features.AddChargeStation.v1.AddChargeStation.Of(
            fakeGroup.Id.Value,
            duplicateChargeStation.Id.Value,
            "Duplicate Charge Station",
            duplicateChargeStation.Connectors.ToConnectorsDto()
        );

        var handler = Scope.ServiceProvider.GetRequiredService<AddChargeStationHandler>();

        // Act & Assert
        var exception = await Should.ThrowAsync<DomainException>(() =>
            handler.Handle(addChargeStation, CancellationToken.None)
        );

        exception.Message.ShouldBe("Charge station with this ID already exists in the group");
    }
}
