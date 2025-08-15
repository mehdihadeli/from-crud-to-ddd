using Microsoft.Extensions.DependencyInjection;
using SmartCharging.TestsShared.Fixtures;
using SmartChargingApi;
using SmartChargingApi.Groups.Contracts;
using SmartChargingApi.Shared.Data;

namespace SmartCharging.IntegrationTests.Groups.Services;

public class GroupStatisticsExternalProviderTests(
    SharedFixture<SmartChargingMetadata, SmartChargingDbContext> sharedFixture
) : SmartChargingIntegrationTestBase(sharedFixture)
{
    [Fact]
    public async Task GetCapacityStatisticsAsync_WhenGroupIdIsValid_ShouldReturnCapacityStatisticsDto()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var (mockResponse, endpoint) = GroupStatisticsExternalServiceMock.SetupGetCapacityStatistics(groupId);

        var provider = Scope.ServiceProvider.GetRequiredService<IGroupStatisticsExternalProvider>();

        // Act
        var result = await provider.GetCapacityStatisticsAsync(groupId);

        // Assert
        result.ShouldNotBeNull();
        result!.MaxCapacityAmps.ShouldBe(mockResponse.MaxCapacityAmps);
        result.CurrentLoadAmps.ShouldBe(mockResponse.CurrentLoadAmps);
        result.AvailableCapacityAmps.ShouldBe(mockResponse.AvailableCapacityAmps);
        endpoint.ShouldBe($"/api/v1/group-statistics/group-capacity/{groupId}");
    }

    [Fact]
    public async Task GetCapacityStatisticsAsync_WhenGroupIdDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var (mockResponse, endpoint) = GroupStatisticsExternalServiceMock.SetupNotFoundCapacityStatistics(groupId);

        var provider = Scope.ServiceProvider.GetRequiredService<IGroupStatisticsExternalProvider>();

        // Act
        var result = await provider.GetCapacityStatisticsAsync(groupId);

        // Assert
        result.ShouldBeNull();
        endpoint.ShouldBe($"/api/v1/group-statistics/group-capacity/{groupId}");
        mockResponse.ShouldBeNull();
    }

    [Fact]
    public async Task GetCapacityStatisticsAsync_WhenServerReturnsNoContent_ShouldReturnNull()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var (mockResponse, endpoint) = GroupStatisticsExternalServiceMock.SetupNoContentCapacityStatistics(groupId);

        var provider = Scope.ServiceProvider.GetRequiredService<IGroupStatisticsExternalProvider>();

        // Act
        var result = await provider.GetCapacityStatisticsAsync(groupId);

        // Assert
        result.ShouldBeNull();
        endpoint.ShouldBe($"/api/v1/group-statistics/group-capacity/{groupId}");
        mockResponse.ShouldBeNull();
    }

    [Fact]
    public async Task GetEnergyConsumptionAsync_WhenGroupIdIsValid_ShouldReturnEnergyConsumptionDto()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var (mockResponse, endpoint) = GroupStatisticsExternalServiceMock.SetupGetEnergyStatistics(groupId);

        var provider = Scope.ServiceProvider.GetRequiredService<IGroupStatisticsExternalProvider>();

        // Act
        var result = await provider.GetEnergyConsumptionAsync(groupId);

        // Assert
        result.ShouldNotBeNull();
        result!.EnergyUsedKWh.ShouldBe(mockResponse.EnergyUsedKWh);
        result.PeriodStart.ShouldBe(mockResponse.PeriodStart);
        result.PeriodEnd.ShouldBe(mockResponse.PeriodEnd);
        endpoint.ShouldBe($"/api/v1/group-statistics/group-energy/{groupId}");
    }

    [Fact]
    public async Task GetEnergyConsumptionAsync_WhenGroupIdDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var (mockResponse, endpoint) = GroupStatisticsExternalServiceMock.SetupNotFoundEnergyStatistics(groupId);

        var provider = Scope.ServiceProvider.GetRequiredService<IGroupStatisticsExternalProvider>();

        // Act
        var result = await provider.GetEnergyConsumptionAsync(groupId);

        // Assert
        result.ShouldBeNull();
        endpoint.ShouldBe($"/api/v1/group-statistics/group-energy/{groupId}");
        mockResponse.ShouldBeNull();
    }

    [Fact]
    public async Task GetEnergyConsumptionAsync_WhenServerReturnsNoContent_ShouldReturnNull()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var (mockResponse, endpoint) = GroupStatisticsExternalServiceMock.SetupNoContentEnergyStatistics(groupId);

        var provider = Scope.ServiceProvider.GetRequiredService<IGroupStatisticsExternalProvider>();

        // Act
        var result = await provider.GetEnergyConsumptionAsync(groupId);

        // Assert
        result.ShouldBeNull();
        endpoint.ShouldBe($"/api/v1/group-statistics/group-energy/{groupId}");
        mockResponse.ShouldBeNull();
    }
}
