using SmartCharging.Shared.BuildingBlocks.Exceptions;

namespace SmartCharging.Groups.Models.ValueObjects;

public record ChargeStationId
{
    // For EF materialization - No validation
    // Value object constraints should not be enforced in EF Core materialization and should be enforced during application-level creation with validations (Of)
    private ChargeStationId() { }

    public Guid Value { get; private set; }

    public static ChargeStationId Of(Guid? value)
    {
        if (value == null || value == Guid.Empty)
            throw new ValidationException("Charge Station ID cannot be null or empty");

        return new ChargeStationId { Value = value.Value };
    }

    public static ChargeStationId New()
    {
        return Of(Guid.CreateVersion7());
    }
}
