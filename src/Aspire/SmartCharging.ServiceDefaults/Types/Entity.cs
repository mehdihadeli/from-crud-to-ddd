namespace SmartCharging.ServiceDefaults.Types;

public abstract class Entity<TId>
{
    public TId Id { get; set; } = default!;
}
