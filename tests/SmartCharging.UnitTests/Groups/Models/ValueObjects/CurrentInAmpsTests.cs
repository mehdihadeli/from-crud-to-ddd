using Bogus;
using Shouldly;
using SmartCharging.Groups.Models.ValueObjects;
using ValidationException = SmartCharging.Shared.BuildingBlocks.Exceptions.ValidationException;

namespace SmartCharging.UnitTests.Groups.Models.ValueObjects;

public class CurrentInAmpsTests
{
    private static readonly Faker Faker = new();

    [Fact]
    public void CreateCurrentInAmps_WithValidValue_ShouldSucceed()
    {
        // Arrange
        var validCurrent = Faker.Random.Int(1, 1000);

        // Act
        var current = CurrentInAmps.Of(validCurrent);

        // Assert
        current.Value.ShouldBe(validCurrent);
    }

    [Fact]
    public void CreateCurrentInAmps_WithZeroValue_ShouldThrowValidationException()
    {
        // Act & Assert
        Should
            .Throw<ValidationException>(() => CurrentInAmps.Of(0))
            .Message.ShouldBe("Current `0` must be greater than 0");
    }

    [Fact]
    public void CreateCurrentInAmps_WithNegativeValue_ShouldThrowValidationException()
    {
        // Act & Assert
        Should
            .Throw<ValidationException>(() => CurrentInAmps.Of(-25))
            .Message.ShouldBe("Current `-25` must be greater than 0");
    }

    [Fact]
    public void UpdateCurrentInAmps_WithValidValue_ShouldSucceed()
    {
        // Arrange
        var current = CurrentInAmps.Of(50);
        var newCurrentValue = Faker.Random.Int(1, 500);

        // Act
        var updatedCurrent = current.WithNewCurrent(newCurrentValue);

        // Assert
        updatedCurrent.Value.ShouldBe(newCurrentValue);
    }
}
