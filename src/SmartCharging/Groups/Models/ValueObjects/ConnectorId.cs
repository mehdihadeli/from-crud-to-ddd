using SmartCharging.Shared.BuildingBlocks.Exceptions;

namespace SmartCharging.Groups.Models.ValueObjects;

// - `record` types automatically implement value-based equality based on all of their public properties. This means, in your current implementation:
// The `Value` property (`int Value`) is used to determine equality between two `ConnectorId` instances

public record ConnectorId
{
    // For EF materialization - No validation
    // Value object constraints should not be enforced in EF Core materialization and should be enforced during application-level creation with validations (Of)
    private ConnectorId() { }

    public int Value { get; private set; }

    public static ConnectorId Of(int value)
    {
        if (value < 1 || value > 5)
            throw new ValidationException($"Connector ID must be between 1 and 5; it is currently '{value}'");

        return new ConnectorId { Value = value };
    }
}
