using SmartCharging.Shared.BuildingBlocks.Exceptions;
using SmartCharging.Shared.BuildingBlocks.Types;

namespace SmartCharging.Groups.Models.ValueObjects;

// - `record` types automatically implement value-based equality based on all of their public properties. This means, in your current implementation:
// The `Value` property (`int Value`) is used to determine equality between two `ChargeStationId` instances

public record ChargeStationId : IStronglyTypedId<Guid>
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
