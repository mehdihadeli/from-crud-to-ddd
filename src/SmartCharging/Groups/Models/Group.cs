using SmartCharging.Shared.BuildingBlocks.Types;

namespace SmartCharging.Groups.Models;

/// <summary>
/// representing a charging group that manages charge stations
/// </summary>
public class Group : Entity<Guid>, IAggregateRoot
{
    /// <summary>
    /// Maximum total current capacity (in Amps) for this group
    /// </summary>
    public required int CapacityInAmps { get; set; }

    /// <summary>
    /// Name of the charging group
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Read-only a list of charge stations belonging to this group
    /// </summary>
    public IList<ChargeStation> ChargeStations { get; set; } = new List<ChargeStation>();

    public int GetTotalCurrent() => ChargeStations.Sum(s => s.GetTotalCurrent());
}
