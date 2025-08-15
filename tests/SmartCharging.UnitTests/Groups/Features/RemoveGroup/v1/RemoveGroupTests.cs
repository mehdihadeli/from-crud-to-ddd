using SmartCharging.ServiceDefaults.Exceptions;

namespace SmartCharging.UnitTests.Groups.Features.RemoveGroup.v1;

public class RemoveGroupTests
{
    [Fact]
    public void Of_WithValidGroupId_CreatesInstanceSuccessfully()
    {
        // Arrange
        var groupId = Guid.NewGuid();

        // Act
        var result = SmartChargingApi.Groups.Features.RemoveGroup.v1.RemoveGroup.Of(groupId);

        // Assert
        result.ShouldNotBeNull();
        result.GroupId.ShouldBe(groupId);
    }

    [Fact]
    public void Of_WithNullGroupId_ThrowsValidationException()
    {
        // Arrange
        Guid? nullGroupId = null;

        // Act & Assert
        Should
            .Throw<ValidationException>(() =>
                SmartChargingApi.Groups.Features.RemoveGroup.v1.RemoveGroup.Of(nullGroupId)
            )
            .Message.ShouldContain("groupId cannot be null or empty.");
    }
}
