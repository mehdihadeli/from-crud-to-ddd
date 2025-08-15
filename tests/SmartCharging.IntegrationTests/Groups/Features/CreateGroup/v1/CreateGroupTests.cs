using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartCharging.IntegrationTests.Groups.Mocks;
using SmartCharging.Shared.Data;
using SmartCharging.TestsShared.Fixtures;
using SmartChargingApi;
using SmartChargingApi.Groups;
using SmartChargingApi.Groups.Features.CreateGroup.v1;
using SmartChargingApi.Groups.Models.ValueObjects;
using SmartChargingApi.Shared.Data;

namespace SmartCharging.IntegrationTests.Groups.Features.CreateGroup.v1;

public class CreateGroupTests(SharedFixture<SmartChargingMetadata, SmartChargingDbContext> sharedFixture)
    : SmartChargingIntegrationTestBase(sharedFixture)
{
    [Fact]
    internal async Task CreateGroup_WhenDataIsValid_CreatesGroupSuccessfully()
    {
        // Arrange
        var fakeGroup = new GroupFake(numberOfConnectorsPerStation: 3).Generate();
        var dto = fakeGroup.ToGroupDto();

        var createGroup = SmartChargingApi.Groups.Features.CreateGroup.v1.CreateGroup.Of(
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
    internal async Task CreateGroup_WhenMaxConnectorsAreProvided_PersistsDataProperly()
    {
        // Arrange
        var fakeGroup = new GroupFake(numberOfConnectorsPerStation: 5).Generate();
        var dto = fakeGroup.ToGroupDto();

        var createGroup = SmartChargingApi.Groups.Features.CreateGroup.v1.CreateGroup.Of(
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
