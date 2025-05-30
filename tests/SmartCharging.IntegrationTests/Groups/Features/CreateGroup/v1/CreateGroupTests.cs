using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartCharging.Groups;
using SmartCharging.Groups.Features.CreateGroup.v1;
using SmartCharging.Groups.Models.ValueObjects;
using SmartCharging.IntegrationTests.Groups.Mocks;
using SmartCharging.Shared.Application.Data;
using SmartCharging.TestsShared.Fixtures;
using Xunit.Abstractions;

namespace SmartCharging.IntegrationTests.Groups.Features.CreateGroup.v1;

public class CreateGroupTests(
    SharedFixture<SmartChargingMetadata, SmartChargingDbContext> sharedFixture,
    ITestOutputHelper outputHelper
) : SmartChargingIntegrationTestBase(sharedFixture, outputHelper)
{
    [Fact]
    internal async Task CreateGroup_WithValidData_Should_CreateGroupSuccessfully()
    {
        // Arrange
        var fakeGroup = new GroupFake(numberOfConnectorsPerStation: 3).Generate();
        var dto = fakeGroup.ToGroupDto();

        var createGroup = SmartCharging.Groups.Features.CreateGroup.v1.CreateGroup.Of(
            dto.Name,
            dto.CapacityInAmps,
            dto.ChargeStations.First()
        );
        var handler = Scope.ServiceProvider.GetRequiredService<CreateGroupHandler>();

        // Act
        var result = await handler.Handle(createGroup, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.GroupId.ShouldNotBe(Guid.Empty);

        // Verify group creation
        var createdGroup = await SharedFixture.ExecuteEfDbContextAsync(async db =>
        {
            return await db
                .Groups.Include(g => g.ChargeStations)
                .ThenInclude(cs => cs.Connectors)
                .FirstOrDefaultAsync(g => g.Id == GroupId.Of(result.GroupId));
        });

        createdGroup.ShouldNotBeNull();
        createdGroup.Name.Value.ShouldBe(fakeGroup.Name.Value);
        createdGroup.CapacityInAmps.Value.ShouldBe(fakeGroup.CapacityInAmps.Value);

        // Additional assertions for specific charge station configurations
        createdGroup.ChargeStations.ShouldNotBeNull();
        createdGroup.ChargeStations.Count.ShouldBe(1);
        createdGroup.ChargeStations.First().Connectors.Count.ShouldBe(3); // Verifying 3 connectors
    }

    [Fact]
    internal async Task CreateGroup_WithMaxConnectors_Should_PersistDataProperly()
    {
        // Arrange
        var fakeGroup = new GroupFake(numberOfConnectorsPerStation: 5).Generate();
        var dto = fakeGroup.ToGroupDto();

        var createGroup = SmartCharging.Groups.Features.CreateGroup.v1.CreateGroup.Of(
            dto.Name,
            dto.CapacityInAmps,
            dto.ChargeStations.First()
        );
        var handler = Scope.ServiceProvider.GetRequiredService<CreateGroupHandler>();

        // Act
        var result = await handler.Handle(createGroup, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.GroupId.ShouldNotBe(Guid.Empty);

        // Verify the group and its data in the database
        var createdGroup = await SharedFixture.ExecuteEfDbContextAsync(async db =>
        {
            return await db
                .Groups.Include(g => g.ChargeStations)
                .ThenInclude(cs => cs.Connectors)
                .FirstOrDefaultAsync(g => g.Id == GroupId.Of(result.GroupId));
        });

        createdGroup.ShouldNotBeNull();
        createdGroup.ChargeStations.First().Connectors.Count.ShouldBe(5);
    }
}
