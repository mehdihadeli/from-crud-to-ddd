using SmartCharging.ServiceDefaults.Exceptions;

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
        var result = SmartChargingApi.Groups.Features.UpdateGroup.v1.UpdateGroup.Of(groupId, name, capacityInAmps);

        // Assert
        result.ShouldNotBeNull();
        result.GroupId.ShouldBe(groupId);
        result.Name.ShouldBe(name);
        result.CapacityInAmps.ShouldBe(capacityInAmps);
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
                SmartChargingApi.Groups.Features.UpdateGroup.v1.UpdateGroup.Of(nullGroupId, name, capacityInAmps)
            )
            .Message.ShouldContain("groupId cannot be null or empty.");
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
                SmartChargingApi.Groups.Features.UpdateGroup.v1.UpdateGroup.Of(groupId, nullName, capacityInAmps)
            )
            .Message.ShouldContain("name cannot be null or empty.");
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
                SmartChargingApi.Groups.Features.UpdateGroup.v1.UpdateGroup.Of(groupId, name, invalidCapacity)
            )
            .Message.ShouldContain("capacityInAmps cannot be negative or zero.");
    }
}
