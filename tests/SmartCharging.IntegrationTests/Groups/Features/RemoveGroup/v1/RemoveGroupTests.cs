using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartCharging.IntegrationTests.Groups.Mocks;
using SmartCharging.ServiceDefaults.Exceptions;
using SmartCharging.Shared.Data;
using SmartCharging.TestsShared.Fixtures;
using SmartChargingApi;
using SmartChargingApi.Groups.Features.RemoveGroup.v1;
using SmartChargingApi.Shared.Data;

namespace SmartCharging.IntegrationTests.Groups.Features.RemoveGroup.v1;

public class RemoveGroupTests(SharedFixture<SmartChargingMetadata, SmartChargingDbContext> sharedFixture)
    : SmartChargingIntegrationTestBase(sharedFixture)
{
    [Fact]
    internal async Task RemoveGroup_WithValidInputs_Should_RemoveGroupAndAssociatedEntities()
    {
        // Arrange
        var fakeGroup = new GroupFake(numberOfConnectorsPerStation: 3).Generate();

        await SharedFixture.ExecuteEfDbContextAsync(async db =>
        {
            await db.Groups.AddAsync(fakeGroup);
            await db.SaveChangesAsync();
        });

        var removeGroup = SmartChargingApi.Groups.Features.RemoveGroup.v1.RemoveGroup.Of(fakeGroup.Id.Value);
        var handler = Scope.ServiceProvider.GetRequiredService<RemoveGroupHandler>();

        // Act
        await handler.Handle(removeGroup, CancellationToken.None);

        // Assert
        var groupExists = await SharedFixture.ExecuteEfDbContextAsync(async db =>
        {
            return await db.Groups.AnyAsync(g => g.Id == fakeGroup.Id);
        });

        groupExists.ShouldBeFalse();
    }

    [Fact]
    internal async Task RemoveGroup_WithNonExistentGroupId_Should_ThrowNotFoundException()
    {
        // Arrange
        var nonExistentGroupId = Guid.NewGuid();

        var removeGroup = SmartChargingApi.Groups.Features.RemoveGroup.v1.RemoveGroup.Of(nonExistentGroupId);
        var handler = Scope.ServiceProvider.GetRequiredService<RemoveGroupHandler>();

        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(() =>
            handler.Handle(removeGroup, CancellationToken.None)
        );

        exception.Message.ShouldBe($"Group with ID {nonExistentGroupId} not found.");
    }

    [Fact]
    internal async Task RemoveGroup_WithNullInputs_Should_ThrowValidationException()
    {
        // Act & Assert
        var handler = Scope.ServiceProvider.GetRequiredService<RemoveGroupHandler>();

        var exception = await Should.ThrowAsync<ValidationException>(() =>
            handler.Handle(null!, CancellationToken.None)
        );

        exception.Message.ShouldBe("removeGroup cannot be null or empty.");
    }

    [Fact]
    internal async Task RemoveGroup_WithGroupHavingMultipleChargeStations_Should_RemoveGroupAndStationsSuccessfully()
    {
        // Arrange
        var fakeGroup = new GroupFake(numberOfConnectorsPerStation: 3).Generate();
        var additionalChargeStation = new ChargeStationFake(2).Generate();
        fakeGroup.AddChargeStation(additionalChargeStation);

        await SharedFixture.ExecuteEfDbContextAsync(async db =>
        {
            await db.Groups.AddAsync(fakeGroup);
            await db.SaveChangesAsync();
        });

        var removeGroup = SmartChargingApi.Groups.Features.RemoveGroup.v1.RemoveGroup.Of(fakeGroup.Id.Value);
        var handler = Scope.ServiceProvider.GetRequiredService<RemoveGroupHandler>();

        // Act
        await handler.Handle(removeGroup, CancellationToken.None);

        // Assert
        var groupExists = await SharedFixture.ExecuteEfDbContextAsync(async db =>
        {
            return await db.Groups.AnyAsync(g => g.Id == fakeGroup.Id);
        });

        groupExists.ShouldBeFalse();
    }
}
