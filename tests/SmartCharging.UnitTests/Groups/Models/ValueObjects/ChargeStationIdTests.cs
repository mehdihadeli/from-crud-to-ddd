using Bogus;
using SmartChargingApi.Groups.Models.ValueObjects;
using ValidationException = SmartCharging.ServiceDefaults.Exceptions.ValidationException;

namespace SmartCharging.UnitTests.Groups.Models.ValueObjects;

public class ChargeStationIdTests
{
    private static readonly Faker Faker = new();

    [Fact]
    public void CreateChargeStationId_WithValidGuid_ShouldSucceed()
    {
        // Arrange
        var validId = Guid.NewGuid();

        // Act
        var chargeStationId = ChargeStationId.Of(validId);

        // Assert
        chargeStationId.Value.ShouldBe(validId);
    }

    [Fact]
    public void CreateChargeStationId_WithEmptyGuid_ShouldThrowValidationException()
    {
        // Act & Assert
        Should
            .Throw<ValidationException>(() => ChargeStationId.Of(Guid.Empty))
            .Message.ShouldBe("Charge Station ID cannot be null or empty");
    }

    [Fact]
    public void CreateNewChargeStationId_ShouldGenerateNewGuid()
    {
        // Act
        var chargeStationId = ChargeStationId.New();

        // Assert
        chargeStationId.Value.ShouldNotBe(Guid.Empty);
    }
}
