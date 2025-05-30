using Shouldly;
using SmartCharging.Shared.BuildingBlocks.Exceptions;

namespace SmartCharging.UnitTests.Groups.Features.GroupGetById.v1;

public class GroupGetByIdTests
{
    [Fact]
    public void Of_WithValidGuid_CreatesInstanceSuccessfully()
    {
        // Arrange
        var validGuid = Guid.NewGuid();

        // Act
        var result = SmartCharging.Groups.Features.GroupGetById.v1.GroupGetById.Of(validGuid);

        // Assert
        result.ShouldNotBeNull();
        result.GroupId.Value.ShouldBe(validGuid);
    }

    [Fact]
    public void Of_WithNullGuid_ThrowsValidationException()
    {
        // Arrange
        Guid? nullGuid = null;

        // Act & Assert
        Should
            .Throw<ValidationException>(() => SmartCharging.Groups.Features.GroupGetById.v1.GroupGetById.Of(nullGuid))
            .Message.ShouldContain("Group ID cannot be null or empty");
    }
}
