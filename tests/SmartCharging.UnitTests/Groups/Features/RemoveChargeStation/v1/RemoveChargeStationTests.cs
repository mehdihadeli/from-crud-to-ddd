using SmartCharging.Shared.BuildingBlocks.Exceptions;

namespace SmartCharging.UnitTests.Groups.Features.RemoveChargeStation.v1;

public class RemoveChargeStationTests
{
    [Fact]
    public void Of_WithValidGroupIdAndChargeStationId_CreatesInstanceSuccessfully()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var chargeStationId = Guid.NewGuid();

        // Act
        var result = SmartCharging.Groups.Features.RemoveChargeStation.v1.RemoveChargeStation.Of(
            groupId,
            chargeStationId
        );

        // Assert
        result.ShouldNotBeNull();
        result.GroupId.ShouldBe(groupId);
        result.ChargeStationId.ShouldBe(chargeStationId);
    }

    [Fact]
    public void Of_WithNullGroupId_ThrowsValidationException()
    {
        // Arrange
        Guid? nullGroupId = null;
        var chargeStationId = Guid.NewGuid();

        // Act & Assert
        Should
            .Throw<ValidationException>(() =>
                SmartCharging.Groups.Features.RemoveChargeStation.v1.RemoveChargeStation.Of(
                    nullGroupId,
                    chargeStationId
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

        // Act & Assert
        Should
            .Throw<ValidationException>(() =>
                SmartCharging.Groups.Features.RemoveChargeStation.v1.RemoveChargeStation.Of(
                    groupId,
                    nullChargeStationId
                )
            )
            .Message.ShouldContain("chargeStationId cannot be null or empty.");
    }
}
