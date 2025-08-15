using SmartCharging.ServiceDefaults.Exceptions;
using SmartChargingApi.Groups.Features.GetGroupById.v1;

namespace SmartCharging.UnitTests.Groups.Features.GroupGetById.v1;

public class GetGroupByIdTests
{
    [Fact]
    public void Of_WithValidGuid_CreatesInstanceSuccessfully()
    {
        // Arrange
        var validGuid = Guid.NewGuid();

        // Act
        var result = GetGroupById.Of(validGuid);

        // Assert
        result.ShouldNotBeNull();
        result.GroupId.ShouldBe(validGuid);
    }

    [Fact]
    public void Of_WithNullGuid_ThrowsValidationException()
    {
        // Arrange
        Guid? nullGuid = null;

        // Act & Assert
        Should
            .Throw<ValidationException>(() => GetGroupById.Of(nullGuid))
            .Message.ShouldContain("groupId cannot be null or empty.");
    }
}
