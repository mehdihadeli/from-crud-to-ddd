using SmartCharging.Shared.BuildingBlocks.Exceptions;

namespace SmartCharging.Groups.Models.ValueObjects;

public record CurrentInAmps
{
    // For EF materialization - No validation
    // Value object constraints should not be enforced in EF Core materialization and should be enforced during application-level creation with validations (Of)
    private CurrentInAmps() { }

    public int Value { get; private set; }

    public static CurrentInAmps Of(int value)
    {
        if (value <= 0)
            throw new ValidationException($"Current `{value}` must be greater than 0");

        return new CurrentInAmps { Value = value };
    }

    public CurrentInAmps WithNewCurrent(int newValue) => Of(newValue);
}
