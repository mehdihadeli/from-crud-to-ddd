using SmartCharging.Shared.BuildingBlocks.Types;

namespace SmartCharging.Groups.Models;

/// <summary>
/// Entity representing a charge station that contains 1-5 connectors.
/// Can only be modified through its parent Group aggregate root, exists only within a Group, Has its own identity (ChargeStationId), Manages its connectors
/// </summary>
public class ChargeStation : Entity<Guid>
{
    /// <summary>
    /// Name of the charge station
    /// </summary>
    public required string Name { get; set; }

    public required Guid GroupId { get; set; }

    /// <summary>
    /// List of connectors in this station
    /// </summary>
    public IList<Connector> Connectors { get; set; } = new List<Connector>();

    /// <summary>
    /// Calculates total current across all connectors
    /// </summary>
    public int GetTotalCurrent() => Connectors.Sum(c => c.MaxCurrentInAmps);
}
