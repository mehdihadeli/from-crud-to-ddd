using SmartCharging.Groups.Models;
using SmartCharging.Groups.Models.ValueObjects;
using SmartCharging.Shared.BuildingBlocks.Exceptions;

namespace SmartCharging.UnitTests.Groups.Features.AddStationConnectors.v1;

public class AddStationConnectorsTests
{
    [Fact]
    public void AddConnectors_ToExistingChargeStation_AddsConnectorsSuccessfully()
    {
        // Arrange: Create a group with one charge station
        var groupName = Name.Of("Group A");
        var groupCapacityInAmps = CurrentInAmps.Of(500);
        var chargeStationId = ChargeStationId.New();

        var initialConnectors = new List<Connector>
        {
            Connector.Create(ConnectorId.Of(1), CurrentInAmps.Of(50)),
        }.AsReadOnly();

        var chargeStation = ChargeStation.Create(chargeStationId, Name.Of("Station A"), initialConnectors);
        var group = Group.Create(GroupId.New(), groupName, groupCapacityInAmps, chargeStation);

        // Create new connectors to add
        var newConnectors = new List<Connector>
        {
            Connector.Create(ConnectorId.Of(2), CurrentInAmps.Of(50)),
            Connector.Create(ConnectorId.Of(3), CurrentInAmps.Of(30)),
        };

        // Act: Add the connectors to the charge station in the group
        group.AddConnectors(chargeStationId, newConnectors);

        // Assert
        var station = group.ChargeStations.First(st => st.Id == chargeStationId);
        station.Connectors.Count.ShouldBe(3);
        station.Connectors.ShouldContain(connector =>
            connector.Id == ConnectorId.Of(2) && connector.MaxCurrentInAmps == CurrentInAmps.Of(50)
        );
    }

    [Fact]
    public void AddConnectors_WhenTotalCurrentExceedsCapacity_ThrowsDomainException()
    {
        // Arrange: Create a group with a restricted capacity
        var groupName = Name.Of("Group B");
        var groupCapacityInAmps = CurrentInAmps.Of(150);
        var chargeStationId = ChargeStationId.New();

        var existingConnector = Connector.Create(ConnectorId.Of(1), CurrentInAmps.Of(100));
        var chargeStation = ChargeStation.Create(
            chargeStationId,
            Name.Of("Station B"),
            new List<Connector> { existingConnector }.AsReadOnly()
        );

        var group = Group.Create(GroupId.New(), groupName, groupCapacityInAmps, chargeStation);

        // Try adding new connectors whose total current exceeds group capacity
        var newConnectors = new List<Connector> { Connector.Create(ConnectorId.Of(2), CurrentInAmps.Of(70)) };

        // Act & Assert: Adding exceeding connectors should throw DomainException
        var exception = Should.Throw<DomainException>(() => group.AddConnectors(chargeStationId, newConnectors));
        exception.Message.ShouldBe("Adding these connectors would exceed group's capacity of 150 amps.");
    }

    [Fact]
    public void AddConnectors_WithDuplicateIds_ThrowsDomainException()
    {
        // Arrange: Create a group with one charge station containing a connector
        var groupName = Name.Of("Group Duplicates");
        var groupCapacityInAmps = CurrentInAmps.Of(500);
        var chargeStationId = ChargeStationId.New();

        var existingConnector = Connector.Create(ConnectorId.Of(1), CurrentInAmps.Of(100));
        var chargeStation = ChargeStation.Create(
            chargeStationId,
            Name.Of("Station C"),
            new List<Connector> { existingConnector }.AsReadOnly()
        );

        var group = Group.Create(GroupId.New(), groupName, groupCapacityInAmps, chargeStation);

        // Try adding a connector with a duplicate ID
        var newConnectors = new List<Connector>
        {
            Connector.Create(ConnectorId.Of(1), CurrentInAmps.Of(50)), // Duplicate ID
        };

        // Act & Assert: Adding a duplicate connector should throw DomainException
        var exception = Should.Throw<DomainException>(() => group.AddConnectors(chargeStationId, newConnectors));
        exception.Message.ShouldBe("Connector IDs must be unique within a charge station.");
    }
}
