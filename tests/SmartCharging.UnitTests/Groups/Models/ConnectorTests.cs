using Shouldly;
using SmartCharging.Groups.Models;
using SmartCharging.Groups.Models.ValueObjects;
using SmartCharging.Shared.BuildingBlocks.Exceptions;

namespace SmartCharging.UnitTests.Groups.Models;

public class ConnectorTests
{
    [Fact]
    public void CreateConnector_WithValidData_ShouldSucceed()
    {
        // Arrange
        var connectorId = ConnectorId.Of(1);
        var maxCurrent = CurrentInAmps.Of(10);
        var chargeStationId = ChargeStationId.New();

        // Act
        var connector = Connector.Create(connectorId, maxCurrent, chargeStationId);

        // Assert
        connector.Id.ShouldBe(connectorId);
        connector.MaxCurrentInAmps.ShouldBe(maxCurrent);
        connector.ChargeStationId.ShouldBe(chargeStationId);
    }

    [Fact]
    public void UpdateCurrentInAmps_WithValidData_ShouldUpdateSuccessfully()
    {
        // Arrange
        var connector = Connector.Create(ConnectorId.Of(1), CurrentInAmps.Of(10), ChargeStationId.New());
        var updatedCurrent = CurrentInAmps.Of(20);

        // Act
        connector.UpdateCurrentInAmps(updatedCurrent);

        // Assert
        connector.MaxCurrentInAmps.ShouldBe(updatedCurrent);
    }

    [Fact]
    public void CreateConnector_WithNullId_ShouldThrowValidationException()
    {
        // Arrange
        CurrentInAmps maxCurrent = CurrentInAmps.Of(10);
        var chargeStationId = ChargeStationId.New();

        // Act & Assert
        Should
            .Throw<ValidationException>(() => Connector.Create(null!, maxCurrent, chargeStationId))
            .Message.ShouldBe("id cannot be null or empty.");
    }

    [Fact]
    public void CreateConnector_WithNullMaxCurrent_ShouldThrowValidationException()
    {
        // Arrange
        var connectorId = ConnectorId.Of(1);
        var chargeStationId = ChargeStationId.New();

        // Act & Assert
        Should
            .Throw<ValidationException>(() => Connector.Create(connectorId, null!, chargeStationId))
            .Message.ShouldBe("maxCurrentInAmps cannot be null or empty.");
    }

    [Fact]
    public void CreateConnector_WithNullChargeStationId_ShouldThrowValidationException()
    {
        // Arrange
        var connectorId = ConnectorId.Of(1);
        var maxCurrent = CurrentInAmps.Of(10);

        // Act & Assert
        Should
            .Throw<ValidationException>(() => Connector.Create(connectorId, maxCurrent, null!))
            .Message.ShouldBe("chargeStationId cannot be null or empty.");
    }

    [Fact]
    public void UpdateCurrentInAmps_WithNullValue_ShouldThrowValidationException()
    {
        // Arrange
        var connector = Connector.Create(ConnectorId.Of(1), CurrentInAmps.Of(10), ChargeStationId.New());

        // Act & Assert
        Should
            .Throw<ValidationException>(() => connector.UpdateCurrentInAmps(null!))
            .Message.ShouldBe("newCurrent cannot be null or empty.");
    }

    [Fact]
    public void CreateConnector_WithInvalidCurrentInAmps_ShouldThrowValidationException()
    {
        // Arrange
        var connectorId = ConnectorId.Of(1);
        var chargeStationId = ChargeStationId.New();

        // Act & Assert
        Should
            .Throw<ValidationException>(() => Connector.Create(connectorId, CurrentInAmps.Of(0), chargeStationId))
            .Message.ShouldBe("Current `0` must be greater than 0");
    }

    [Fact]
    public void UpdateCurrentInAmps_WithInvalidCurrent_ShouldThrowValidationException()
    {
        // Arrange
        var connector = Connector.Create(ConnectorId.Of(1), CurrentInAmps.Of(10), ChargeStationId.New());

        // Act & Assert
        Should
            .Throw<ValidationException>(() => connector.UpdateCurrentInAmps(CurrentInAmps.Of(-5)) // Invalid current
            )
            .Message.ShouldBe("Current `-5` must be greater than 0");
    }
}
