using SmartCharging.Shared.BuildingBlocks.Types;

namespace SmartCharging.Groups.Models;

/// <summary>
/// Entity representing a physical connector in a charge station.
/// Identified by its position (1-5) within the station, The Connector exists only within a ChargeStation and has its own identity
/// </summary>
public class Connector : Entity<int>
{
    public required int MaxCurrentInAmps { get; set; }
    public required Guid ChargeStationId { get; set; }
}
