using SmartCharging.ServiceDefaults.Exceptions;

namespace SmartChargingApi.Groups.Models.ValueObjects;

// - `record` types automatically implement value-based equality based on all of their public properties. This means, in your current implementation:
// The `Value` property (`int Value`) is used to determine equality between two `Name` instances

// https://learn.microsoft.com/en-us/ef/core/modeling/constructors#read-only-properties
public record Name
{
    // For EF materialization - No validation
    // Value object constraints should not be enforced in EF Core materialization and should be enforced during application-level creation with validations (Of)
    private Name() { }

    public string Value { get; private set; }

    public static Name Of(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ValidationException("Name cannot be null or empty");

        if (value.Length > 100)
            throw new ValidationException("Name cannot be longer than 100 characters");

        return new Name { Value = value };
    }

    public Name WithNewName(string? newValue) => Of(newValue);
}
