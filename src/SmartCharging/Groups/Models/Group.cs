using SmartCharging.Groups.Models.ValueObjects;
using SmartCharging.Shared.BuildingBlocks.Exceptions;
using SmartCharging.Shared.BuildingBlocks.Extensions;
using SmartCharging.Shared.BuildingBlocks.Types;

namespace SmartCharging.Groups.Models;

// Navigation flows one-way: Group → ChargeStation → Connector

/// <summary>
/// Aggregate root representing a charging group that manages charge stations and enforces
/// business rules about capacity and connector limits. Group controls access to all child entities.
/// </summary>
public class Group : Entity<GroupId>, IAggregateRoot
{
    /// - The `_chargeStations` collection in `Group` already manages which `ChargeStation` belongs to it, so there’s no need for the child entity (`ChargeStation`) to track the parent with `GroupId`.
    private readonly List<ChargeStation> _chargeStations = new();

    // For EF materialization - No validation
    private Group() { }

    private Group(GroupId id, Name name, CurrentInAmps capacityInAmps)
    {
        Id = id;
        Name = name;
        CapacityInAmps = capacityInAmps;
    }

    /// <summary>
    /// Maximum total current capacity (in Amps) for this group
    /// </summary>
    public CurrentInAmps CapacityInAmps { get; private set; }

    /// <summary>
    /// Name of the charging group
    /// </summary>
    public Name Name { get; private set; }

    /// <summary>
    /// Read-only a list of charge stations belonging to this group
    /// </summary>
    public IReadOnlyCollection<ChargeStation> ChargeStations => _chargeStations.AsReadOnly();

    #region Group Operations

    /// <summary>
    /// Creates a new charging group
    /// </summary>
    public static Group Create(GroupId id, Name name, CurrentInAmps capacityInAmps, ChargeStation? chargeStation)
    {
        id.NotBeNull();
        name.NotBeNull();
        capacityInAmps.NotBeNull();

        var group = new Group(id, name, capacityInAmps);

        if (chargeStation is not null)
        {
            group.AddChargeStation(chargeStation);
        }

        return group;
    }

    /// <summary>
    /// Updates group name
    /// </summary>
    public void UpdateName(Name newName)
    {
        Name = newName.NotBeNull();
    }

    /// <summary>
    /// Updates group capacity after validating it's not less than the current usage
    /// </summary>
    public void UpdateCapacity(CurrentInAmps newCapacity)
    {
        if (newCapacity.Value < GetTotalCurrent())
            throw new DomainException("New capacity cannot be less than current usage");

        CapacityInAmps = newCapacity;
    }

    /// <summary>
    /// Updates group name and capacity in one go.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="capacity"></param>
    public void UpdateGroup(Name name, CurrentInAmps capacity)
    {
        UpdateName(name);
        UpdateCapacity(capacity);
    }

    #endregion

    #region Charge Station Operations

    /// <summary>
    /// Adds a new charge station to the group. Only one Charge Station can be added to a Group in one call.
    /// </summary>
    /// <param name="chargeStation"></param>
    /// <exception cref="DomainException"></exception>
    public void AddChargeStation(ChargeStation chargeStation)
    {
        chargeStation.NotBeNull();

        if (_chargeStations.Any(s => s.Id == chargeStation.Id))
            throw new DomainException("Charge station with this ID already exists in the group");

        if (chargeStation.Connectors.Count < 1)
            throw new DomainException("Charge station must have at least one connector");

        int newTotalCurrent = GetTotalCurrent() + chargeStation.GetTotalCurrent();
        if (newTotalCurrent > CapacityInAmps.Value)
            throw new DomainException("Adding this station would exceed group capacity");

        _chargeStations.Add(chargeStation);
    }

    /// <summary>
    /// Removes a charge station from the group. Only one Charge Station can be removed to a Group in one call.
    /// </summary>
    public void RemoveChargeStation(ChargeStationId stationId)
    {
        stationId.NotBeNull();

        var station = _chargeStations.FirstOrDefault(s => s.Id == stationId);
        if (station == null)
            throw new DomainException("Charge station not found in this group");

        _chargeStations.Remove(station);
    }

    /// <summary>
    /// Updates a charge station's name.
    /// </summary>
    /// <param name="stationId"></param>
    /// <param name="newName"></param>
    /// <exception cref="DomainException"></exception>
    public void UpdateChargeStationName(ChargeStationId stationId, Name newName)
    {
        var station = _chargeStations.FirstOrDefault(s => s.Id == stationId);
        if (station == null)
            throw new DomainException("Charge station not found");

        station.UpdateName(newName);
    }

    #endregion

    #region Connector Operations


    /// <summary>
    /// Adds multiple connectors to a charge station within the group.
    /// </summary>
    /// <param name="stationId"></param>
    /// <param name="connectors"></param>
    /// <exception cref="DomainException"></exception>
    public void AddConnectors(ChargeStationId stationId, IReadOnlyCollection<Connector> connectors)
    {
        var station = _chargeStations.FirstOrDefault(s => s.Id == stationId);
        if (station is null)
            throw new DomainException("Charge station not found.");

        // Validate group constraints before allowing addition
        int additionalCurrent = connectors.Sum(c => c.MaxCurrentInAmps.Value);
        if (GetTotalCurrent() + additionalCurrent > CapacityInAmps.Value)
            throw new DomainException(
                $"Adding these connectors would exceed group's capacity of {CapacityInAmps.Value} amps."
            );

        station.AddConnectors(connectors);
    }

    /// <summary>
    /// Removes multiple connectors from a charge station within the group.
    /// </summary>
    /// <param name="stationId"></param>
    /// <param name="connectorIds"></param>
    /// <exception cref="DomainException"></exception>
    public void RemoveConnectors(ChargeStationId stationId, IReadOnlyCollection<ConnectorId> connectorIds)
    {
        stationId.NotBeNull();
        connectorIds.NotBeNull();

        var station = _chargeStations.FirstOrDefault(s => s.Id == stationId);
        if (station == null)
            throw new DomainException("Charge station not found");

        // Validate station-level constraints and remove connectors
        station.RemoveConnectors(connectorIds);
    }

    /// <summary>
    /// Updates a connector's maximum CurrentInAmps.
    /// </summary>
    public void UpdateConnectorCurrentInAmps(
        ChargeStationId stationId,
        ConnectorId connectorId,
        CurrentInAmps newCurrent
    )
    {
        // Input validation (unchanged)
        stationId.NotBeNull();
        connectorId.NotBeNull();
        newCurrent.NotBeNull();

        var station = _chargeStations.FirstOrDefault(s => s.Id == stationId);
        if (station == null)
            throw new DomainException("Charge station not found");

        var connector = station.Connectors.FirstOrDefault(c => c.Id == connectorId);
        if (connector == null)
            throw new DomainException("Connector not found");

        // Calculate what the new total would be
        int newTotal = (GetTotalCurrent() - connector.MaxCurrentInAmps.Value) + newCurrent.Value;

        // Validate before making any changes
        if (newTotal > CapacityInAmps.Value)
        {
            throw new DomainException(
                $"Updating this connector would exceed group capacity by {newTotal - CapacityInAmps.Value} amps"
            );
        }

        // Only modify if validation passes
        connector.UpdateCurrentInAmps(newCurrent);
    }

    #endregion

    private int GetTotalCurrent() => _chargeStations.Sum(s => s.GetTotalCurrent());
}
