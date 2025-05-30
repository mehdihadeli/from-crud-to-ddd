using SmartCharging.Shared.BuildingBlocks.Exceptions;

namespace SmartCharging.UnitTests.Groups.Features.RemoveGroup.v1;

public class RemoveGroupTests
{
    [Fact]
    public void Of_WithValidGroupId_CreatesInstanceSuccessfully()
    {
        // Arrange
        var groupId = Guid.NewGuid();

        // Act
        var result = SmartCharging.Groups.Features.RemoveGroup.v1.RemoveGroup.Of(groupId);

        // Assert
        result.ShouldNotBeNull();
        result.GroupId.Value.ShouldBe(groupId);
    }

    [Fact]
    public void Of_WithNullGroupId_ThrowsValidationException()
    {
        // Arrange
        Guid? nullGroupId = null;

        // Act & Assert
        Should
            .Throw<ValidationException>(() => SmartCharging.Groups.Features.RemoveGroup.v1.RemoveGroup.Of(nullGroupId))
            .Message.ShouldContain("Group ID cannot be null or empty");
    }
}
