using Shouldly;
using SmartCharging.Groups.Models;
using SmartCharging.Groups.Models.ValueObjects;
using SmartCharging.Shared.BuildingBlocks.Exceptions;
using SmartCharging.UnitTests.Groups.Mocks;

namespace SmartCharging.UnitTests.Groups.Models;

public class ChargeStationTests
{
    [Fact]
    public void CreateChargeStation_WithValidData_ShouldSucceed()
    {
        // Arrange
        var stationId = ChargeStationId.New();
        var stationName = Name.Of("Station 1");
        var connectors = new[]
        {
            Connector.Create(ConnectorId.Of(1), CurrentInAmps.Of(10), stationId),
            Connector.Create(ConnectorId.Of(2), CurrentInAmps.Of(20), stationId),
        };

        // Act
        var chargeStation = ChargeStation.Create(stationId, stationName, connectors);

        // Assert
        chargeStation.Id.ShouldBe(stationId);
        chargeStation.Name.ShouldBe(stationName);
        chargeStation.Connectors.Count.ShouldBe(connectors.Length);
    }

    [Fact]
    public void UpdateChargeStationName_WithValidName_ShouldUpdateSuccessfully()
    {
        // Arrange
        var chargeStation = new ChargeStationFake(2).Generate();
        var updatedName = Name.Of("Updated Station Name");

        // Act
        chargeStation.UpdateName(updatedName);

        // Assert
        chargeStation.Name.ShouldBe(updatedName);
    }

    [Fact]
    public void AddConnectors_WithValidData_ShouldAddSuccessfully()
    {
        // Arrange
        var chargeStation = new ChargeStationFake(1).Generate();
        var additionalConnectors = new[]
        {
            Connector.Create(ConnectorId.Of(2), CurrentInAmps.Of(15), chargeStation.Id),
            Connector.Create(ConnectorId.Of(3), CurrentInAmps.Of(25), chargeStation.Id),
        };

        // Act
        chargeStation.AddConnectors(additionalConnectors);

        // Assert
        chargeStation.Connectors.Count.ShouldBe(3); // 1 existing + 2 new
    }

    [Fact]
    public void RemoveConnectors_WithValidIds_ShouldRemoveSuccessfully()
    {
        // Arrange
        var chargeStation = new ChargeStationFake(3).Generate();
        var connectorsToRemove = chargeStation.Connectors.Take(2).Select(c => c.Id).ToList();

        // Act
        chargeStation.RemoveConnectors(connectorsToRemove);

        // Assert
        chargeStation.Connectors.Count.ShouldBe(1);
    }

    [Fact]
    public void UpdateConnectorCurrent_WithValidData_ShouldUpdateSuccessfully()
    {
        // Arrange
        var chargeStation = new ChargeStationFake(1).Generate();
        var connector = chargeStation.Connectors.First();
        var updatedCurrent = CurrentInAmps.Of(50);

        // Act
        chargeStation.UpdateConnectorCurrentInAmps(connector.Id, updatedCurrent);

        // Assert
        connector.MaxCurrentInAmps.ShouldBe(updatedCurrent);
    }

    [Fact]
    public void GetTotalCurrent_ShouldReturnSumOfAllConnectors()
    {
        // Arrange
        var connectors = new[]
        {
            Connector.Create(ConnectorId.Of(1), CurrentInAmps.Of(10), ChargeStationId.New()),
            Connector.Create(ConnectorId.Of(2), CurrentInAmps.Of(20), ChargeStationId.New()),
            Connector.Create(ConnectorId.Of(3), CurrentInAmps.Of(15), ChargeStationId.New()),
        };
        var chargeStation = ChargeStation.Create(ChargeStationId.New(), Name.Of("Station"), connectors);

        // Act
        var totalCurrent = chargeStation.GetTotalCurrent();

        // Assert
        totalCurrent.ShouldBe(45); // 10 + 20 + 15
    }

    [Fact]
    public void CreateChargeStation_WithNullId_ShouldThrowValidationException()
    {
        // Arrange
        var name = Name.Of("Station");
        var connectors = new[] { Connector.Create(ConnectorId.Of(1), CurrentInAmps.Of(10), ChargeStationId.New()) };

        // Act & Assert
        Should
            .Throw<ValidationException>(() => ChargeStation.Create(null!, name, connectors))
            .Message.ShouldBe("id cannot be null or empty.");
    }

    [Fact]
    public void CreateChargeStation_WithNullName_ShouldThrowValidationException()
    {
        // Arrange
        var id = ChargeStationId.New();
        var connectors = new[] { Connector.Create(ConnectorId.Of(1), CurrentInAmps.Of(10), id) };

        // Act & Assert
        Should
            .Throw<ValidationException>(() => ChargeStation.Create(id, null!, connectors))
            .Message.ShouldBe("name cannot be null or empty.");
    }

    [Fact]
    public void CreateChargeStation_WithNullConnectors_ShouldThrowValidationException()
    {
        // Arrange
        var id = ChargeStationId.New();
        var name = Name.Of("Station");

        // Act & Assert
        Should
            .Throw<ValidationException>(() => ChargeStation.Create(id, name, null!))
            .Message.ShouldBe("connectors cannot be null or empty.");
    }

    [Fact]
    public void AddConnectors_WithDuplicateConnectorIds_ShouldThrowDomainException()
    {
        // Arrange
        var chargeStation = new ChargeStationFake(2).Generate();
        var existingConnector = chargeStation.Connectors.First();

        var duplicateConnector = Connector.Create(existingConnector.Id, CurrentInAmps.Of(25), chargeStation.Id);

        // Act & Assert
        Should
            .Throw<DomainException>(() => chargeStation.AddConnectors(new[] { duplicateConnector }))
            .Message.ShouldBe("Connector IDs must be unique within a charge station.");
    }

    [Fact]
    public void AddConnectors_ExceedingLimit_ShouldThrowDomainException()
    {
        // Arrange
        var chargeStation = new ChargeStationFake(4).Generate();
        var additionalConnectors = new[]
        {
            Connector.Create(ConnectorId.Of(5), CurrentInAmps.Of(10), chargeStation.Id),
            Connector.Create(ConnectorId.Of(4), CurrentInAmps.Of(20), chargeStation.Id), // Exceeds limit
        };

        // Act & Assert
        Should
            .Throw<DomainException>(() => chargeStation.AddConnectors(additionalConnectors))
            .Message.ShouldBe("Cannot have more than 5 connectors in a charge station. It is 6.");
    }

    [Fact]
    public void RemoveConnectors_RemovingAll_ShouldThrowDomainException()
    {
        // Arrange
        var chargeStation = new ChargeStationFake(2).Generate();
        var allConnectorIds = chargeStation.Connectors.Select(c => c.Id).ToList();

        // Act & Assert
        Should
            .Throw<DomainException>(() => chargeStation.RemoveConnectors(allConnectorIds))
            .Message.ShouldBe("Removing these connectors would leave the charge station without any connectors");
    }

    [Fact]
    public void RemoveConnectors_WithNonExistentConnectorId_ShouldThrowDomainException()
    {
        // Arrange
        var chargeStation = new ChargeStationFake(2).Generate();
        var nonExistentConnectorId = ConnectorId.Of(3); // Not in the charge station

        // Act & Assert
        Should
            .Throw<DomainException>(() => chargeStation.RemoveConnectors(new[] { nonExistentConnectorId }))
            .Message.ShouldStartWith("The following connectors do not exist:");
    }

    [Fact]
    public void UpdateConnectorCurrentInAmps_WithNonExistentConnectorId_ShouldThrowDomainException()
    {
        // Arrange
        var chargeStation = new ChargeStationFake(1).Generate();
        var nonExistentConnectorId = ConnectorId.Of(3); // Not in the charge station
        var updatedCurrent = CurrentInAmps.Of(50);

        // Act & Assert
        Should
            .Throw<DomainException>(() =>
                chargeStation.UpdateConnectorCurrentInAmps(nonExistentConnectorId, updatedCurrent)
            )
            .Message.ShouldBe("Connector not found");
    }
}
