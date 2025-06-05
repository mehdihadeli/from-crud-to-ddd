namespace SmartCharging.Shared.BuildingBlocks.Types;

public abstract class Entity<TId>
{
    public required TId Id { get; init; } = default!;
}
