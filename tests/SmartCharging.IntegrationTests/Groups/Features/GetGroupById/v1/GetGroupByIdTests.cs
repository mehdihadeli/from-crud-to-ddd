using Microsoft.Extensions.DependencyInjection;
using SmartCharging.IntegrationTests.Groups.Mocks;
using SmartCharging.ServiceDefaults.Exceptions;
using SmartCharging.TestsShared.Fixtures;
using SmartChargingApi;
using SmartChargingApi.Groups.Features.GetGroupById.v1;
using SmartChargingApi.Shared.Data;

namespace SmartCharging.IntegrationTests.Groups.Features.GetGroupById.v1;

public class GetGroupByIdTests(SharedFixture<SmartChargingMetadata, SmartChargingDbContext> sharedFixture)
    : SmartChargingIntegrationTestBase(sharedFixture)
{
    [Fact]
    internal async Task CreateGroup_WhenDataIsValid_ShouldCreateGroupSuccessfully()
    {
        // Arrange
        var fakeGroup = new GroupFake().Generate(1).First();
        await SharedFixture.InsertEfDbContextAsync(fakeGroup);

        var (capacityStatsDto, _) = GroupStatisticsExternalServiceMock.SetupGetCapacityStatistics(fakeGroup.Id.Value);
        var (energyStatsDto, _) = GroupStatisticsExternalServiceMock.SetupGetEnergyStatistics(fakeGroup.Id.Value);

        var handler = Scope.ServiceProvider.GetRequiredService<GetGroupByIdHandler>();
        var query = new SmartChargingApi.Groups.Features.GetGroupById.v1.GetGroupById(fakeGroup.Id.Value);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Group.ShouldNotBeNull();
        result.Group.GroupId.ShouldBe(fakeGroup.Id.Value);
        result.Group.Name.ShouldBe(fakeGroup.Name.Value);
        result.Group.CapacityInAmps.ShouldBe(fakeGroup.CapacityInAmps.Value);
        result.CapacityStats.ShouldNotBeNull();
        result.CapacityStats!.MaxCapacityAmps.ShouldBe(capacityStatsDto.MaxCapacityAmps);
        result.EnergyStats.ShouldNotBeNull();
        result.EnergyStats!.EnergyUsedKWh.ShouldBe(energyStatsDto.EnergyUsedKWh);
    }

    [Fact]
    internal async Task CreateGroup_WhenMaxConnectorsAreProvided_ShouldPersistDataProperly()
    {
        var fakeGroup = new GroupFake().Generate(1).First();
        await SharedFixture.InsertEfDbContextAsync(fakeGroup);

        GroupStatisticsExternalServiceMock.SetupNoContentCapacityStatistics(fakeGroup.Id.Value);
        GroupStatisticsExternalServiceMock.SetupNoContentEnergyStatistics(fakeGroup.Id.Value);

        var handler = Scope.ServiceProvider.GetRequiredService<GetGroupByIdHandler>();
        var query = new SmartChargingApi.Groups.Features.GetGroupById.v1.GetGroupById(fakeGroup.Id.Value);

        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        result.ShouldNotBeNull();
        result.Group.ShouldNotBeNull();
        result.CapacityStats.ShouldBeNull();
        result.EnergyStats.ShouldBeNull();
    }

    [Fact]
    internal async Task Handle_WhenGroupExistsAndBothStatsNotFound_ReturnsGroupWithNullStatistics()
    {
        var fakeGroup = new GroupFake().Generate(1).First();
        await SharedFixture.InsertEfDbContextAsync(fakeGroup);

        GroupStatisticsExternalServiceMock.SetupNotFoundCapacityStatistics(fakeGroup.Id.Value);
        GroupStatisticsExternalServiceMock.SetupNotFoundEnergyStatistics(fakeGroup.Id.Value);

        var handler = Scope.ServiceProvider.GetRequiredService<GetGroupByIdHandler>();
        var query = new SmartChargingApi.Groups.Features.GetGroupById.v1.GetGroupById(fakeGroup.Id.Value);

        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        result.ShouldNotBeNull();
        result.Group.ShouldNotBeNull();
        result.CapacityStats.ShouldBeNull();
        result.EnergyStats.ShouldBeNull();
    }

    [Fact]
    internal async Task Handle_WhenGroupExistsAndOnlyCapacityStatsPresent_ReturnsGroupWithOnlyCapacityStats()
    {
        var fakeGroup = new GroupFake().Generate(1).First();
        await SharedFixture.InsertEfDbContextAsync(fakeGroup);

        var (capacityStatsDto, _) = GroupStatisticsExternalServiceMock.SetupGetCapacityStatistics(fakeGroup.Id.Value);
        GroupStatisticsExternalServiceMock.SetupNoContentEnergyStatistics(fakeGroup.Id.Value);

        var handler = Scope.ServiceProvider.GetRequiredService<GetGroupByIdHandler>();
        var query = new SmartChargingApi.Groups.Features.GetGroupById.v1.GetGroupById(fakeGroup.Id.Value);

        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        result.ShouldNotBeNull();
        result.Group.ShouldNotBeNull();
        result.CapacityStats.ShouldNotBeNull();
        result.CapacityStats!.MaxCapacityAmps.ShouldBe(capacityStatsDto.MaxCapacityAmps);
        result.EnergyStats.ShouldBeNull();
    }

    [Fact]
    internal async Task Handle_WhenGroupExistsAndOnlyEnergyStatsPresent_ReturnsGroupWithOnlyEnergyStats()
    {
        var fakeGroup = new GroupFake().Generate(1).First();
        await SharedFixture.InsertEfDbContextAsync(fakeGroup);

        GroupStatisticsExternalServiceMock.SetupNoContentCapacityStatistics(fakeGroup.Id.Value);
        var (energyStatsDto, _) = GroupStatisticsExternalServiceMock.SetupGetEnergyStatistics(fakeGroup.Id.Value);

        var handler = Scope.ServiceProvider.GetRequiredService<GetGroupByIdHandler>();
        var query = new SmartChargingApi.Groups.Features.GetGroupById.v1.GetGroupById(fakeGroup.Id.Value);

        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        result.ShouldNotBeNull();
        result.Group.ShouldNotBeNull();
        result.CapacityStats.ShouldBeNull();
        result.EnergyStats.ShouldNotBeNull();
        result.EnergyStats!.EnergyUsedKWh.ShouldBe(energyStatsDto.EnergyUsedKWh);
    }

    [Fact]
    internal async Task Handle_WhenGroupDoesNotExist_ThrowsNotFoundException()
    {
        var groupId = Guid.NewGuid();
        var handler = Scope.ServiceProvider.GetRequiredService<GetGroupByIdHandler>();
        var query = new SmartChargingApi.Groups.Features.GetGroupById.v1.GetGroupById(groupId);

        await Should.ThrowAsync<NotFoundException>(() => handler.Handle(query, TestContext.Current.CancellationToken));
    }

    [Fact]
    internal async Task Handle_WhenInputIsNull_ThrowsValidationException()
    {
        var handler = Scope.ServiceProvider.GetRequiredService<GetGroupByIdHandler>();
        await Should.ThrowAsync<ValidationException>(() =>
            handler.Handle(null!, TestContext.Current.CancellationToken)
        );
    }
}
