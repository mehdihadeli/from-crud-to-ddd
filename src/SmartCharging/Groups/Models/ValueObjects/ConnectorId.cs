using SmartCharging.Shared.BuildingBlocks.Exceptions;

namespace SmartCharging.Groups.Models.ValueObjects;

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
