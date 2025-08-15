using SmartCharging.ServiceDefaults.Extensions;
using SmartCharging.ServiceDefaults.Types;
using SmartChargingApi.Groups.Models.ValueObjects;

namespace SmartChargingApi.Groups.Models;

// Note: Connector doesn't need to know about its parent to enforce business rules, Any validation requiring parent data is handled by ChargeStation or Group and we can manage the relationship without a physical reference (Shadow property).

/// <summary>
/// Entity representing a physical connector in a charge station.
/// Identified by its position (1-5) within the station, The Connector exists only within a ChargeStation and has its own identity
/// </summary>
public class Connector : Entity<ConnectorId>
{
    // For EF materialization - No validation
    private Connector() { }

    private Connector(ConnectorId id, CurrentInAmps maxCurrentInAmps)
    {
        Id = id.NotBeNull();
        MaxCurrentInAmps = maxCurrentInAmps.NotBeNull();
    }

    /// <summary>
    /// Maximum allowable current (in Amps) for this connector
    /// </summary>
    public CurrentInAmps MaxCurrentInAmps { get; private set; }

    // will be set by ChargeStation connectors navigation by ef core, we don't need to set it explicitly
    public ChargeStationId ChargeStationId { get; private set; } = default!;

    /// <summary>
    /// Creates a new connector with the given maximum current
    /// </summary>
    /// <param name="id"></param>
    /// <param name="maxCurrentInAmps"></param>
    /// <returns></returns>
    public static Connector Create(ConnectorId id, CurrentInAmps maxCurrentInAmps)
    {
        id.NotBeNull();
        maxCurrentInAmps.NotBeNull();

        return new Connector(id, maxCurrentInAmps);
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
