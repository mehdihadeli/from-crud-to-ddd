using Bogus;
using Shouldly;
using SmartCharging.Groups.Models.ValueObjects;
using ValidationException = SmartCharging.Shared.BuildingBlocks.Exceptions.ValidationException;

namespace SmartCharging.UnitTests.Groups.Models.ValueObjects;

public class GroupIdTests
{
    private static readonly Faker Faker = new();

    [Fact]
    public void CreateGroupId_WithValidGuid_ShouldSucceed()
    {
        // Arrange
        var validId = Guid.NewGuid();

        // Act
        var groupId = GroupId.Of(validId);

        // Assert
        groupId.Value.ShouldBe(validId);
    }

    [Fact]
    public void CreateGroupId_WithEmptyGuid_ShouldThrowValidationException()
    {
        // Act & Assert
        Should
            .Throw<ValidationException>(() => GroupId.Of(Guid.Empty))
            .Message.ShouldBe("Group ID cannot be null or empty");
    }

    [Fact]
    public void CreateNewGroupId_ShouldGenerateNewGuid()
    {
        // Act
        var groupId = GroupId.New();

        // Assert
        groupId.Value.ShouldNotBe(Guid.Empty);
    }
}
