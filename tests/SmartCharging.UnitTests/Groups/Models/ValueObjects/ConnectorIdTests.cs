using Bogus;
using SmartChargingApi.Groups.Models.ValueObjects;
using ValidationException = SmartCharging.ServiceDefaults.Exceptions.ValidationException;

namespace SmartCharging.UnitTests.Groups.Models.ValueObjects;

public class ConnectorIdTests
{
    private static readonly Faker Faker = new();

    [Fact]
    public void CreateConnectorId_WithValidValue_ShouldSucceed()
    {
        // Arrange
        var validConnectorId = Faker.Random.Int(1, 5);

        // Act
        var connectorId = ConnectorId.Of(validConnectorId);

        // Assert
        connectorId.Value.ShouldBe(validConnectorId);
    }

    [Fact]
    public void CreateConnectorId_WithValueLessThanOne_ShouldThrowValidationException()
    {
        // Act & Assert
        Should
            .Throw<ValidationException>(() => ConnectorId.Of(0))
            .Message.ShouldBe("Connector ID must be between 1 and 5; it is currently '0'");
    }

    [Fact]
    public void CreateConnectorId_WithValueGreaterThanFive_ShouldThrowValidationException()
    {
        // Act & Assert
        Should
            .Throw<ValidationException>(() => ConnectorId.Of(6))
            .Message.ShouldBe("Connector ID must be between 1 and 5; it is currently '6'");
    }
}
