using SmartCharging.Groups.Models.ValueObjects;
using SmartCharging.Shared.BuildingBlocks.Exceptions;
using SmartCharging.Shared.BuildingBlocks.Extensions;
using SmartCharging.Shared.BuildingBlocks.Types;

namespace SmartCharging.Groups.Models;

/// <summary>
/// Entity representing a charge station that contains 1-5 connectors.
/// Can only be modified through its parent Group aggregate root, exists only within a Group, Has its own identity (ChargeStationId), Manages its connectors
/// </summary>
public class ChargeStation : Entity<ChargeStationId>
{
    private readonly List<Connector> _connectors = new();

    // For EF materialization - No validation
    private ChargeStation() { }

    private ChargeStation(ChargeStationId id, Name name)
    {
        Id = id;
        Name = name;
    }

    /// <summary>
    /// Name of the charge station
    /// </summary>
    public Name Name { get; private set; }

    /// <summary>
    /// Read-only list of connectors in this station
    /// </summary>
    public IReadOnlyCollection<Connector> Connectors => _connectors.AsReadOnly();

    // will be set by group stations navigation by ef core, we don't need to set it explicitly
    public GroupId GroupId { get; private set; } = default!;

    /// <summary>
    /// Creates a new charge station with connectors.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="connectors"></param>
    /// <returns></returns>
    public static ChargeStation Create(ChargeStationId id, Name name, IReadOnlyCollection<Connector> connectors)
    {
        id.NotBeNull();
        name.NotBeNull();
        connectors.NotBeNull();

        var station = new ChargeStation(id, name);

        station.AddConnectors(connectors);

        return station;
    }

    /// <summary>
    /// Updates the charge station's name.
    /// </summary>
    /// <param name="name"></param>
    public void UpdateName(Name name)
    {
        name.NotBeNull();

        Name = name;
    }

    public void AddConnectors(IReadOnlyCollection<Connector> connectors)
    {
        connectors.NotBeNull();

        // Validate the total number of connectors after addition - ChargeStation should have at least one, but not more than 5 connectors.
        if (_connectors.Count + connectors.Count > 5)
            throw new DomainException(
                $"Cannot have more than 5 connectors in a charge station. It is {_connectors.Count + connectors.Count}."
            );

        // Ensure the uniqueness of connector IDs (within both existing and new connectors)
        var newConnectorIds = connectors.Select(c => c.Id.Value).ToList();
        var existingConnectorIds = _connectors.Select(c => c.Id.Value).ToList();

        if (newConnectorIds.Intersect(existingConnectorIds).Any())
            throw new DomainException("Connector IDs must be unique within a charge station.");

        // Add new connectors to the existing list
        _connectors.AddRange(connectors);
    }

    /// <summary>
    /// Removes multiple connectors from the station.
    /// </summary>
    /// <param name="connectorIds">Collection of connector IDs to remove</param>
    /// <exception cref="DomainException">Thrown if constraints are violated</exception>
    public void RemoveConnectors(IReadOnlyCollection<ConnectorId> connectorIds)
    {
        connectorIds.NotBeNull();

        // Validate that all specified IDs exist in the current connectors
        var existingIds = _connectors.Select(c => c.Id).ToHashSet();
        var nonExistentIds = connectorIds.Where(id => !existingIds.Contains(id)).ToList();
        if (nonExistentIds.Any())
            throw new DomainException($"The following connectors do not exist: {string.Join(", ", nonExistentIds)}");

        // Validate that at least one connector will remain after the removal
        if (_connectors.Count - connectorIds.Count < 1)
            throw new DomainException(
                "Removing these connectors would leave the charge station without any connectors"
            );

        _connectors.RemoveAll(c => connectorIds.Contains(c.Id));
    }

    /// <summary>
    /// Updates a connector's maximum current in amps.
    /// </summary>
    /// <param name="connectorId"></param>
    /// <param name="newCurrent"></param>
    /// <exception cref="DomainException">Throws if constraints are violated</exception>
    public void UpdateConnectorCurrentInAmps(ConnectorId connectorId, CurrentInAmps newCurrent)
    {
        connectorId.NotBeNull();
        newCurrent.NotBeNull();

        var connector = _connectors.FirstOrDefault(c => c.Id == connectorId);
        if (connector == null)
            throw new DomainException("Connector not found");

        // Ensure the new current value is greater than zero
        if (newCurrent.Value <= 0)
            throw new DomainException("Connector max current must be greater than zero");

        connector.UpdateCurrentInAmps(newCurrent);
    }

    /// <summary>
    /// Calculates total current across all connectors
    /// </summary>
    public int GetTotalCurrent() => _connectors.Sum(c => c.MaxCurrentInAmps.Value);
}
