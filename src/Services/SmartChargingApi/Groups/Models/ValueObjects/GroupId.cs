using SmartCharging.ServiceDefaults.Exceptions;
using SmartCharging.ServiceDefaults.Types;

namespace SmartChargingApi.Groups.Models.ValueObjects;

// - `record` types automatically implement value-based equality based on all of their public properties. This means, in your current implementation:
// The `Value` property (`int Value`) is used to determine equality between two `GroupId` instances

public record GroupId : IStronglyTypedId<Guid>
{
    // For EF materialization - No validation
    // Value object constraints should not be enforced in EF Core materialization and should be enforced during application-level creation with validations (Of)
    private GroupId() { }

    public Guid Value { get; private set; }

    public static GroupId Of(Guid? value)
    {
        if (value == null || value == Guid.Empty)
            throw new ValidationException("Group ID cannot be null or empty");

        return new GroupId { Value = value.Value };
    }

    public static GroupId New()
    {
        return Of(Guid.CreateVersion7());
    }
}
