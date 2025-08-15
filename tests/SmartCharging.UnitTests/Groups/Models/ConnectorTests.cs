using SmartCharging.ServiceDefaults.Exceptions;
using SmartChargingApi.Groups.Models;
using SmartChargingApi.Groups.Models.ValueObjects;

namespace SmartCharging.UnitTests.Groups.Models;

public class ConnectorTests
{
    [Fact]
    public void CreateConnector_WithValidData_ShouldSucceed()
    {
        // Arrange
        var connectorId = ConnectorId.Of(1);
        var maxCurrent = CurrentInAmps.Of(10);

        // Act
        var connector = Connector.Create(connectorId, maxCurrent);

        // Assert
        connector.Id.ShouldBe(connectorId);
        connector.MaxCurrentInAmps.ShouldBe(maxCurrent);
        connector.ChargeStationId.ShouldBeNull();
    }

    [Fact]
    public void UpdateCurrentInAmps_WithValidData_ShouldUpdateSuccessfully()
    {
        // Arrange
        var connector = Connector.Create(ConnectorId.Of(1), CurrentInAmps.Of(10));
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
            .Throw<ValidationException>(() => Connector.Create(null!, maxCurrent))
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
            .Throw<ValidationException>(() => Connector.Create(connectorId, null!))
            .Message.ShouldBe("maxCurrentInAmps cannot be null or empty.");
    }

    [Fact]
    public void UpdateCurrentInAmps_WithNullValue_ShouldThrowValidationException()
    {
        // Arrange
        var connector = Connector.Create(ConnectorId.Of(1), CurrentInAmps.Of(10));

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
            .Throw<ValidationException>(() => Connector.Create(connectorId, CurrentInAmps.Of(0)))
            .Message.ShouldBe("Current `0` must be greater than 0");
    }

    [Fact]
    public void UpdateCurrentInAmps_WithInvalidCurrent_ShouldThrowValidationException()
    {
        // Arrange
        var connector = Connector.Create(ConnectorId.Of(1), CurrentInAmps.Of(10));

        // Act & Assert
        Should
            .Throw<ValidationException>(() =>
                connector.UpdateCurrentInAmps(CurrentInAmps.Of(-5)) // Invalid current
            )
            .Message.ShouldBe("Current `-5` must be greater than 0");
    }
}
