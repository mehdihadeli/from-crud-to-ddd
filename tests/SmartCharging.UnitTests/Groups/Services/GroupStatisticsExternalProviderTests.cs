using System.Net;
using System.Text.Json;
using RichardSzalay.MockHttp;
using SmartCharging.TestsShared.Extensions;
using SmartChargingApi.Groups.Dtos.Clients;
using SmartChargingApi.Groups.Services;

namespace SmartCharging.UnitTests.Groups.Services;

public class GroupStatisticsExternalProviderTests
{
    private readonly MockHttpMessageHandler _mockHttp;
    private readonly HttpClient _httpClient;
    private readonly GroupStatisticsExternalProvider _provider;

    public GroupStatisticsExternalProviderTests()
    {
        _mockHttp = new MockHttpMessageHandler();
        _httpClient = _mockHttp.ToHttpClient();
        _httpClient.BaseAddress = new Uri("http://localhost");
        _provider = new GroupStatisticsExternalProvider(_httpClient);
    }

    #region GetCapacityStatisticsAsync

    [Fact]
    public async Task GetCapacityStatisticsAsync_WhenGroupIdIsValid_ShouldReturnCapacityStatisticsDto()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var expectedClientResponse = new GroupCapacityStatisticsClientResponseDto(groupId, 45, 100, 55);
        _mockHttp
            .When(HttpMethod.Get, $"/api/v1/group-statistics/group-capacity/{groupId}")
            .RespondJson(expectedClientResponse);

        // Act
        var result = await _provider.GetCapacityStatisticsAsync(groupId);

        // Assert
        result.ShouldNotBeNull();
        result!.CurrentLoadAmps.ShouldBe(expectedClientResponse.CurrentLoadAmps);
        result.MaxCapacityAmps.ShouldBe(expectedClientResponse.MaxCapacityAmps);
        result.AvailableCapacityAmps.ShouldBe(expectedClientResponse.AvailableCapacityAmps);
    }

    [Fact]
    public async Task GetCapacityStatisticsAsync_WhenGroupIdDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        _mockHttp
            .When(HttpMethod.Get, $"/api/v1/group-statistics/group-capacity/{groupId}")
            .Respond(HttpStatusCode.NotFound);

        // Act
        var result = await _provider.GetCapacityStatisticsAsync(groupId);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetCapacityStatisticsAsync_WhenServerReturnsNoContent_ShouldReturnNull()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        _mockHttp
            .When(HttpMethod.Get, $"/api/v1/group-statistics/group-capacity/{groupId}")
            .Respond(HttpStatusCode.NoContent);

        // Act
        var result = await _provider.GetCapacityStatisticsAsync(groupId);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetCapacityStatisticsAsync_WhenHttpError_ShouldThrowHttpRequestException()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        _mockHttp
            .When(HttpMethod.Get, $"/api/v1/group-statistics/group-capacity/{groupId}")
            .Respond(HttpStatusCode.InternalServerError);

        // Act & Assert
        await Should.ThrowAsync<HttpRequestException>(() => _provider.GetCapacityStatisticsAsync(groupId));
    }

    [Fact]
    public async Task GetCapacityStatisticsAsync_WhenInvalidJson_ShouldThrowJsonException()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        _mockHttp
            .When(HttpMethod.Get, $"/api/v1/group-statistics/group-capacity/{groupId}")
            .Respond("application/json", "not a json");

        // Act & Assert
        await Should.ThrowAsync<JsonException>(() => _provider.GetCapacityStatisticsAsync(groupId));
    }

    #endregion

    #region GetEnergyConsumptionAsync

    [Fact]
    public async Task GetEnergyConsumptionAsync_WhenGroupIdIsValid_ShouldReturnEnergyConsumptionDto()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var expectedClientResponse = new GroupEnergyConsumptionClientResponseDto(
            groupId,
            1234.56,
            DateTime.Today.AddDays(-7),
            DateTime.Today
        );
        _mockHttp
            .When(HttpMethod.Get, $"/api/v1/group-statistics/group-energy/{groupId}")
            .RespondJson(expectedClientResponse);

        // Act
        var result = await _provider.GetEnergyConsumptionAsync(groupId);

        // Assert
        result.ShouldNotBeNull();
        result!.EnergyUsedKWh.ShouldBe(expectedClientResponse.EnergyUsedKWh);
        result.PeriodStart.ShouldBe(expectedClientResponse.PeriodStart);
        result.PeriodEnd.ShouldBe(expectedClientResponse.PeriodEnd);
    }

    [Fact]
    public async Task GetEnergyConsumptionAsync_WhenGroupIdDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        _mockHttp
            .When(HttpMethod.Get, $"/api/v1/group-statistics/group-energy/{groupId}")
            .Respond(HttpStatusCode.NotFound);

        // Act
        var result = await _provider.GetEnergyConsumptionAsync(groupId);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetEnergyConsumptionAsync_WhenServerReturnsNoContent_ShouldReturnNull()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        _mockHttp
            .When(HttpMethod.Get, $"/api/v1/group-statistics/group-energy/{groupId}")
            .Respond(HttpStatusCode.NoContent);

        // Act
        var result = await _provider.GetEnergyConsumptionAsync(groupId);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetEnergyConsumptionAsync_WhenHttpError_ShouldThrowHttpRequestException()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        _mockHttp
            .When(HttpMethod.Get, $"/api/v1/group-statistics/group-energy/{groupId}")
            .Respond(HttpStatusCode.InternalServerError);

        // Act & Assert
        await Should.ThrowAsync<HttpRequestException>(() => _provider.GetEnergyConsumptionAsync(groupId));
    }

    [Fact]
    public async Task GetEnergyConsumptionAsync_WhenInvalidJson_ShouldThrowJsonException()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        _mockHttp
            .When(HttpMethod.Get, $"/api/v1/group-statistics/group-energy/{groupId}")
            .Respond("application/json", "bad response");

        // Act & Assert
        await Should.ThrowAsync<JsonException>(() => _provider.GetEnergyConsumptionAsync(groupId));
    }

    #endregion
}
