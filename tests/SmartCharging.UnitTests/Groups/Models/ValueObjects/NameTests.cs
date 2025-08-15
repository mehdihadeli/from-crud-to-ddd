using SmartCharging.ServiceDefaults.Exceptions;
using SmartChargingApi.Groups.Models.ValueObjects;

namespace SmartCharging.UnitTests.Groups.Models.ValueObjects;

public class NameTests
{
    private static readonly Bogus.Faker Faker = new();

    [Fact]
    public void CreateName_WithValidValue_ShouldSucceed()
    {
        // Arrange
        var fakeName = Faker.Commerce.ProductName();

        // Act
        var name = Name.Of(fakeName);

        // Assert
        name.Value.ShouldBe(fakeName);
    }

    [Fact]
    public void CreateName_WithEmptyValue_ShouldThrowValidationException()
    {
        // Act & Assert
        Should
            .Throw<ValidationException>(() => Name.Of(string.Empty))
            .Message.ShouldBe("Name cannot be null or empty");
    }

    [Fact]
    public void CreateName_WithExceedinglyLongValue_ShouldThrowValidationException()
    {
        // Arrange
        // Name exceeding 100 characters
        var longName = Faker.Random.String(102);

        // Act & Assert
        Should
            .Throw<ValidationException>(() => Name.Of(longName))
            .Message.ShouldBe("Name cannot be longer than 100 characters");
    }
}
