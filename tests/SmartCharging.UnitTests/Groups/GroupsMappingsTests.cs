using SmartCharging.UnitTests.Groups.Mocks;
using SmartChargingApi.Groups;
using SmartChargingApi.Groups.Dtos;

namespace SmartCharging.UnitTests.Groups;

public class GroupsMappingsTests
{
    [Fact]
    public void ToConnectorDto_WithValidConnector_CreatesConnectorDto()
    {
        // Arrange
        var chargeStation = new ChargeStationFake(2).Generate();
        var connector = chargeStation.Connectors.First();

        // Act
        var connectorDto = connector.ToConnectorDto();

        // Assert
        connectorDto.ConnectorId.ShouldBe(connector.Id.Value);
        connectorDto.MaxCurrentInAmps.ShouldBe(connector.MaxCurrentInAmps.Value);
    }

    [Fact]
    public void ToConnectorsDto_WithValidConnectors_CreatesConnectorsDto()
    {
        // Arrange
        var chargeStation = new ChargeStationFake(3).Generate();
        var connectors = chargeStation.Connectors;

        // Act
        var connectorsDto = connectors.ToConnectorsDto();

        // Assert
        connectorsDto.Count.ShouldBe(3);
        connectorsDto.First().ConnectorId.ShouldBe(connectors.First().Id.Value);
        connectorsDto.Last().MaxCurrentInAmps.ShouldBe(connectors.Last().MaxCurrentInAmps.Value);
    }

    [Fact]
    public void ToConnector_WithValidConnectorDto_CreatesConnector()
    {
        // Arrange
        var chargeStation = new ChargeStationFake(1).Generate();
        var connectorDto = new ConnectorDto(ConnectorId: 1, MaxCurrentInAmps: 30);

        // Act
        var connector = connectorDto.ToConnector();

        // Assert
        connector.Id.Value.ShouldBe(connectorDto.ConnectorId);
        connector.MaxCurrentInAmps.Value.ShouldBe(connectorDto.MaxCurrentInAmps);
        connector.ChargeStationId.ShouldBeNull();
    }

    [Fact]
    public void ToGroupDto_WithValidGroup_CreatesGroupDto()
    {
        // Arrange
        var group = new GroupFake(3, 300).Generate();

        // Act
        var groupDto = group.ToGroupDto();

        // Assert
        groupDto.GroupId.ShouldBe(group.Id.Value);
        groupDto.Name.ShouldBe(group.Name.Value);
        groupDto.CapacityInAmps.ShouldBe(group.CapacityInAmps.Value);
        groupDto.ChargeStations.First().Name.ShouldBe(group.ChargeStations.First().Name.Value);
    }

    [Fact]
    public void ToGroupGetByIdResult_WithValidGroup_ReturnsCorrectResult()
    {
        // Arrange
        var group = new GroupFake(2, 400).Generate();
        var capacityStats = new GroupCapacityStatisticsDto(
            CurrentLoadAmps: 100,
            MaxCapacityAmps: 200,
            AvailableCapacityAmps: 100
        );

        var energyStats = new GroupEnergyConsumptionDto(
            EnergyUsedKWh: 1500,
            PeriodStart: DateTime.UtcNow.AddDays(-7),
            PeriodEnd: DateTime.UtcNow
        );

        // Act
        var result = group.ToGroupGetByIdResult(capacityStats: capacityStats, energyStats: energyStats);

        // Assert
        result.Group.ShouldNotBeNull();
        result.Group.GroupId.ShouldBe(group.Id.Value);
        result.Group.Name.ShouldBe(group.Name.Value);
        result.Group.CapacityInAmps.ShouldBe(group.CapacityInAmps.Value);
        result.Group.ChargeStations.First().Name.ShouldBe(group.ChargeStations.First().Name.Value);
    }

    [Fact]
    public void ToChargeStation_WithValidChargeStationDto_CreatesChargeStation()
    {
        // Arrange
        var chargeStationId = Guid.NewGuid();
        var chargeStationDto = new ChargeStationDto(
            ChargeStationId: chargeStationId,
            Name: "Test Charge Station",
            Connectors: new List<ConnectorDto>
            {
                new(ConnectorId: 1, MaxCurrentInAmps: 30),
                new(ConnectorId: 2, MaxCurrentInAmps: 40),
            }
        );

        // Act
        var chargeStation = chargeStationDto.ToChargeStation();

        // Assert
        chargeStation.Id.Value.ShouldBe(chargeStationDto.ChargeStationId);
        chargeStation.Name.Value.ShouldBe(chargeStationDto.Name);
        chargeStation.Connectors.Count.ShouldBe(2);
        chargeStation
            .Connectors.First()
            .MaxCurrentInAmps.Value.ShouldBe(chargeStationDto.Connectors.First().MaxCurrentInAmps);
    }

    [Fact]
    public void ToConnectors_WithValidConnectorDtos_CreatesConnectors()
    {
        // Arrange
        var chargeStationId = Guid.NewGuid();
        var connectorDtos = new List<ConnectorDto>
        {
            new(ConnectorId: 1, MaxCurrentInAmps: 30),
            new(ConnectorId: 2, MaxCurrentInAmps: 40),
        };

        // Act
        var connectors = connectorDtos.ToConnectors();

        // Assert
        connectors.Count.ShouldBe(2);
        connectors.First().Id.Value.ShouldBe(connectorDtos.First().ConnectorId);
        connectors.Last().MaxCurrentInAmps.Value.ShouldBe(connectorDtos.Last().MaxCurrentInAmps);
    }
}
