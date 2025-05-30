using SmartCharging.Shared.BuildingBlocks.Exceptions;

namespace SmartCharging.UnitTests.Groups.Features.UpdateConnectorCurrentInAmps.v1;

public class UpdateConnectorCurrentInAmpsTests
{
    [Fact]
    public void Of_WithValidParameters_CreatesInstanceSuccessfully()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var chargeStationId = Guid.NewGuid();
        var connectorId = 1;
        var newCurrent = 25;

        // Act
        var result = SmartCharging.Groups.Features.UpdateConnectorCurrentInAmps.v1.UpdateConnectorCurrentInAmps.Of(
            groupId,
            chargeStationId,
            connectorId,
            newCurrent
        );

        // Assert
        result.ShouldNotBeNull();
        result.GroupId.Value.ShouldBe(groupId);
        result.ChargeStationId.Value.ShouldBe(chargeStationId);
        result.ConnectorId.Value.ShouldBe(connectorId);
        result.NewCurrentInAmps.Value.ShouldBe(newCurrent);
    }

    [Fact]
    public void Of_WithNullGroupId_ThrowsValidationException()
    {
        // Arrange
        Guid? nullGroupId = null;
        var chargeStationId = Guid.NewGuid();
        var connectorId = 1;
        var newCurrent = 25;

        // Act & Assert
        Should
            .Throw<ValidationException>(() =>
                SmartCharging.Groups.Features.UpdateConnectorCurrentInAmps.v1.UpdateConnectorCurrentInAmps.Of(
                    nullGroupId,
                    chargeStationId,
                    connectorId,
                    newCurrent
                )
            )
            .Message.ShouldContain("groupId cannot be null or empty.");
    }

    [Fact]
    public void Of_WithNullChargeStationId_ThrowsValidationException()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        Guid? nullChargeStationId = null;
        var connectorId = 1;
        var newCurrent = 25;

        // Act & Assert
        Should
            .Throw<ValidationException>(() =>
                SmartCharging.Groups.Features.UpdateConnectorCurrentInAmps.v1.UpdateConnectorCurrentInAmps.Of(
                    groupId,
                    nullChargeStationId,
                    connectorId,
                    newCurrent
                )
            )
            .Message.ShouldContain("chargeStationId cannot be null or empty.");
    }

    [Fact]
    public void Of_WithInvalidCurrentInAmps_ThrowsValidationException()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var chargeStationId = Guid.NewGuid();
        var connectorId = 1;

        // Act & Assert
        Should
            .Throw<ValidationException>(() =>
                SmartCharging.Groups.Features.UpdateConnectorCurrentInAmps.v1.UpdateConnectorCurrentInAmps.Of(
                    groupId,
                    chargeStationId,
                    connectorId,
                    0
                )
            ) // Invalid: Current cannot be zero
            .Message.ShouldContain("Current `0` must be greater than 0");
    }

    [Fact]
    public void Of_WithInvalidConnectorId_ThrowsValidationException()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var chargeStationId = Guid.NewGuid();
        var invalidConnectorId = -1;
        var newCurrent = 25;

        // Act & Assert
        Should
            .Throw<ValidationException>(() =>
                SmartCharging.Groups.Features.UpdateConnectorCurrentInAmps.v1.UpdateConnectorCurrentInAmps.Of(
                    groupId,
                    chargeStationId,
                    invalidConnectorId,
                    newCurrent
                )
            )
            .Message.ShouldContain("Connector ID must be between 1 and 5; it is currently '-1'");
    }
}
