using SmartCharging.Shared.BuildingBlocks.Exceptions;

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
        var result = SmartCharging.Groups.Features.UpdateChargeStationName.v1.UpdateChargeStationName.Of(
            groupId,
            chargeStationId,
            newName
        );

        // Assert
        result.ShouldNotBeNull();
        result.GroupId.Value.ShouldBe(groupId);
        result.ChargeStationId.Value.ShouldBe(chargeStationId);
        result.NewName.Value.ShouldBe(newName);
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
                SmartCharging.Groups.Features.UpdateChargeStationName.v1.UpdateChargeStationName.Of(
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
                SmartCharging.Groups.Features.UpdateChargeStationName.v1.UpdateChargeStationName.Of(
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
                SmartCharging.Groups.Features.UpdateChargeStationName.v1.UpdateChargeStationName.Of(
                    groupId,
                    chargeStationId,
                    nullNewName
                )
            )
            .Message.ShouldContain("newName cannot be null or white space.");

        Should
            .Throw<ValidationException>(() =>
                SmartCharging.Groups.Features.UpdateChargeStationName.v1.UpdateChargeStationName.Of(
                    groupId,
                    chargeStationId,
                    "   "
                )
            )
            .Message.ShouldContain("newName cannot be null or white space.");
    }
}
