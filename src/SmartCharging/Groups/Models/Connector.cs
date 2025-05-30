using SmartCharging.Groups.Models.ValueObjects;
using SmartCharging.Shared.BuildingBlocks.Extensions;
using SmartCharging.Shared.BuildingBlocks.Types;

namespace SmartCharging.Groups.Models;

// Note: Connector doesn't need to know about its parent to enforce business rules, Any validation requiring parent data is handled by ChargeStation or Group and we can manage the relationship without a physical reference (Shadow property).

/// <summary>
/// Entity representing a physical connector in a charge station.
/// Identified by its position (1-5) within the station, The Connector exists only within a ChargeStation and has its own identity
/// </summary>
public class Connector : Entity<ConnectorId>
{
    // For EF materialization - No validation
    private Connector() { }

    private Connector(ConnectorId id, CurrentInAmps maxCurrentInAmps, ChargeStationId chargeStationId)
    {
        Id = id.NotBeNull();
        MaxCurrentInAmps = maxCurrentInAmps.NotBeNull();
        ChargeStationId = chargeStationId.NotBeNull();
    }

    /// <summary>
    /// Maximum allowable current (in Amps) for this connector
    /// </summary>
    public CurrentInAmps MaxCurrentInAmps { get; private set; }

    public ChargeStationId ChargeStationId { get; private set; }

    /// <summary>
    /// Creates a new connector with the given maximum current
    /// </summary>
    /// <param name="id"></param>
    /// <param name="maxCurrentInAmps"></param>
    /// <param name="chargeStationId"></param>
    /// <returns></returns>
    public static Connector Create(ConnectorId id, CurrentInAmps maxCurrentInAmps, ChargeStationId chargeStationId)
    {
        id.NotBeNull();
        maxCurrentInAmps.NotBeNull();
        chargeStationId.NotBeNull();

        return new Connector(id, maxCurrentInAmps, chargeStationId);
    }

    /// <summary>
    /// Updates the connector's maximum current
    /// </summary>
    public void UpdateCurrentInAmps(CurrentInAmps newCurrent)
    {
        newCurrent.NotBeNull();
        MaxCurrentInAmps = newCurrent;
    }
}
