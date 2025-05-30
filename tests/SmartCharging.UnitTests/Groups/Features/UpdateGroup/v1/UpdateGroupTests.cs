using SmartCharging.Shared.BuildingBlocks.Exceptions;

namespace SmartCharging.UnitTests.Groups.Features.UpdateGroup.v1;

public class UpdateGroupTests
{
    [Fact]
    public void Of_WithValidParameters_CreatesInstanceSuccessfully()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var name = "Test Group Name";
        var capacityInAmps = 300;

        // Act
        var result = SmartCharging.Groups.Features.UpdateGroup.v1.UpdateGroup.Of(groupId, name, capacityInAmps);

        // Assert
        result.ShouldNotBeNull();
        result.GroupId.Value.ShouldBe(groupId);
        result.Name.Value.ShouldBe(name);
        result.CapacityInAmps.Value.ShouldBe(capacityInAmps);
    }

    [Fact]
    public void Of_WithNullGroupId_ThrowsValidationException()
    {
        // Arrange
        Guid? nullGroupId = null;
        var name = "Test Group Name";
        var capacityInAmps = 300;

        // Act & Assert
        Should
            .Throw<ValidationException>(() =>
                SmartCharging.Groups.Features.UpdateGroup.v1.UpdateGroup.Of(nullGroupId, name, capacityInAmps)
            )
            .Message.ShouldContain("Group ID cannot be null or empty");
    }

    [Fact]
    public void Of_WithNullOrEmptyName_ThrowsValidationException()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        string? nullName = null;
        var capacityInAmps = 300;

        // Act & Assert
        Should
            .Throw<ValidationException>(() =>
                SmartCharging.Groups.Features.UpdateGroup.v1.UpdateGroup.Of(groupId, nullName, capacityInAmps)
            )
            .Message.ShouldContain("Name cannot be null or empty");

        Should
            .Throw<ValidationException>(() =>
                SmartCharging.Groups.Features.UpdateGroup.v1.UpdateGroup.Of(groupId, "   ", capacityInAmps)
            )
            .Message.ShouldContain("Name cannot be null or empty");
    }

    [Fact]
    public void Of_WithInvalidCapacityInAmps_ThrowsValidationException()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var name = "Test Group Name";
        var invalidCapacity = 0;

        // Act & Assert
        Should
            .Throw<ValidationException>(() =>
                SmartCharging.Groups.Features.UpdateGroup.v1.UpdateGroup.Of(groupId, name, invalidCapacity)
            )
            .Message.ShouldContain("Current `0` must be greater than 0");
    }
}
