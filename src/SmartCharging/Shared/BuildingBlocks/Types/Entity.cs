namespace SmartCharging.Shared.BuildingBlocks.Types;

public abstract class Entity<TId>
{
    public TId Id { get; set; } = default!;
}
