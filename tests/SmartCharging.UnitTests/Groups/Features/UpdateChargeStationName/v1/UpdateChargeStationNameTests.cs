using SmartCharging.ServiceDefaults.Exceptions;

namespace SmartCharging.UnitTests.Groups.Features.UpdateChargeStationName.v1;

public class UpdateChargeStationNameTests
{
    [Fact]
    public void Of_WithValidParameters_CreatesInstanceSuccessfully()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var chargeStationId = Guid.NewGuid();
        var newName = "Updated Station Name";

        // Act
        var result = SmartChargingApi.Groups.Features.UpdateChargeStationName.v1.UpdateChargeStationName.Of(
            groupId,
            chargeStationId,
            newName
        );

        // Assert
        result.ShouldNotBeNull();
        result.GroupId.ShouldBe(groupId);
        result.ChargeStationId.ShouldBe(chargeStationId);
        result.NewName.ShouldBe(newName);
    }

    [Fact]
    public void Of_WithNullGroupId_ThrowsValidationException()
    {
        // Arrange
        Guid? nullGroupId = null;
        var chargeStationId = Guid.NewGuid();
        var newName = "Station Name";

        // Act & Assert
        Should
            .Throw<ValidationException>(() =>
                SmartChargingApi.Groups.Features.UpdateChargeStationName.v1.UpdateChargeStationName.Of(
                    nullGroupId,
                    chargeStationId,
                    newName
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
        var newName = "Station Name";

        // Act & Assert
        Should
            .Throw<ValidationException>(() =>
                SmartChargingApi.Groups.Features.UpdateChargeStationName.v1.UpdateChargeStationName.Of(
                    groupId,
                    nullChargeStationId,
                    newName
                )
            )
            .Message.ShouldContain("chargeStationId cannot be null or empty.");
    }

    [Fact]
    public void Of_WithNullOrWhitespaceNewName_ThrowsValidationException()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var chargeStationId = Guid.NewGuid();
        string? nullNewName = null;

        // Act & Assert
        Should
            .Throw<ValidationException>(() =>
                SmartChargingApi.Groups.Features.UpdateChargeStationName.v1.UpdateChargeStationName.Of(
                    groupId,
                    chargeStationId,
                    nullNewName
                )
            )
            .Message.ShouldContain("newName cannot be null or white space.");

        Should
            .Throw<ValidationException>(() =>
                SmartChargingApi.Groups.Features.UpdateChargeStationName.v1.UpdateChargeStationName.Of(
                    groupId,
                    chargeStationId,
                    "   "
                )
            )
            .Message.ShouldContain("newName cannot be null or white space.");
    }
}
