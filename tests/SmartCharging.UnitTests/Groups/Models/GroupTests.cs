using SmartCharging.ServiceDefaults.Exceptions;
using SmartCharging.UnitTests.Groups.Mocks;
using SmartChargingApi.Groups.Models;
using SmartChargingApi.Groups.Models.ValueObjects;

namespace SmartCharging.UnitTests.Groups.Models;

public class GroupTests
{
    [Fact]
    public void CreateGroup_WithValidData_ShouldSucceed()
    {
        // Arrange
        var groupId = GroupId.New();
        var name = Name.Of("Valid Group");
        var capacity = CurrentInAmps.Of(300);
        var chargeStation = new ChargeStationFake(numberOfConnectors: 2).Generate();

        // Act
        var group = Group.Create(groupId, name, capacity, chargeStation);

        // Assert
        group.Id.ShouldBe(groupId);
        group.Name.ShouldBe(name);
        group.CapacityInAmps.ShouldBe(capacity);
        group.ChargeStations.Count.ShouldBe(1);
    }

    [Fact]
    public void CreateGroup_WithoutInitialChargeStation_ShouldSucceed()
    {
        // Arrange
        var groupId = GroupId.New();
        var name = Name.Of("Standalone Group");
        var capacity = CurrentInAmps.Of(300);

        // Act
        var group = Group.Create(groupId, name, capacity, null);

        // Assert
        group.Id.ShouldBe(groupId);
        group.Name.ShouldBe(name);
        group.CapacityInAmps.ShouldBe(capacity);
        group.ChargeStations.Count.ShouldBe(0);
    }

    [Fact]
    public void CreateGroup_WithNullName_ShouldThrowValidationException()
    {
        // Arrange
        var groupId = GroupId.New();
        var capacity = CurrentInAmps.Of(300);
        ChargeStation? chargeStation = null;

        // Act & Assert
        var ex = Should.Throw<ValidationException>(() => Group.Create(groupId, null, capacity, chargeStation));
        ex.Message.ShouldBe("name cannot be null or empty.");
    }

    [Fact]
    public void CreateGroup_WithNullGroupId_ShouldThrowValidationException()
    {
        // Arrange
        Name name = Name.Of("Group with null ID");
        var capacity = CurrentInAmps.Of(300);

        // Act & Assert
        Should
            .Throw<ValidationException>(() => Group.Create(null, name, capacity, null))
            .Message.ShouldBe("id cannot be null or empty.");
    }

    [Fact]
    public void UpdateGroup_WithValidData_ShouldSucceed()
    {
        // Arrange
        var group = new GroupFake().Generate();
        var newName = Name.Of("Updated Name");
        var newCapacity = CurrentInAmps.Of(500);

        // Act
        group.UpdateGroup(newName, newCapacity);

        // Assert
        group.Name.ShouldBe(newName);
        group.CapacityInAmps.ShouldBe(newCapacity);
    }

    [Fact]
    public void UpdateGroupName_WithValidName_ShouldSucceed()
    {
        // Arrange
        var group = new GroupFake().Generate();
        var updatedName = Name.Of("Updated Group Name");

        // Act
        group.UpdateName(updatedName);

        // Assert
        group.Name.ShouldBe(updatedName);
    }

    [Fact]
    public void UpdateCapacity_LessThanCurrentUsage_ShouldThrowDomainException()
    {
        // Arrange
        var group = new GroupFake(3).Generate();
        var usedCapacity = group
            .ChargeStations.SelectMany(station => station.Connectors)
            .Sum(connector => connector.MaxCurrentInAmps.Value);

        var newCapacity = CurrentInAmps.Of(usedCapacity - 10);

        // Act & Assert
        var ex = Should.Throw<DomainException>(() => group.UpdateCapacity(newCapacity));
        ex.Message.ShouldBe("New capacity cannot be less than current usage");
    }

    [Fact]
    public void AddChargeStation_WithValidData_ShouldSucceed()
    {
        // Arrange
        var group = new GroupFake(1, 500, 50).Generate();
        var chargeStation = new ChargeStationFake(1).Generate();

        // Act
        group.AddChargeStation(chargeStation);

        // Assert
        group.ChargeStations.ShouldContain(chargeStation);
        group.ChargeStations.Count.ShouldBe(2);
    }

    [Fact]
    public void AddChargeStation_ExceedingCapacity_ShouldThrowDomainException()
    {
        // Arrange
        var group = new GroupFake(2).Generate();
        var chargeStation = new ChargeStationFake(numberOfConnectors: 2).Generate();

        // Exceed group capacity
        chargeStation.UpdateConnectorCurrentInAmps(
            chargeStation.Connectors.First().Id,
            CurrentInAmps.Of(group.CapacityInAmps.Value)
        );

        // Act & Assert
        var ex = Should.Throw<DomainException>(() => group.AddChargeStation(chargeStation));
        ex.Message.ShouldBe("Adding this station would exceed group capacity");
    }

    [Fact]
    public void RemoveChargeStation_WithValidId_ShouldSucceed()
    {
        // Arrange
        var group = new GroupFake().Generate();
        var stationToRemove = group.ChargeStations.First();

        // Act
        group.RemoveChargeStation(stationToRemove.Id);

        // Assert
        group.ChargeStations.ShouldNotContain(stationToRemove);
    }

    [Fact]
    public void RemoveChargeStation_NonExistentId_ShouldThrowDomainException()
    {
        // Arrange
        var group = new GroupFake().Generate();
        var nonExistentStationId = ChargeStationId.New();

        // Act & Assert
        var ex = Should.Throw<DomainException>(() => group.RemoveChargeStation(nonExistentStationId));
        ex.Message.ShouldBe("Charge station not found in this group");
    }

    [Fact]
    public void RemoveAllChargeStations_Sequentially_ShouldSucceed()
    {
        // Arrange
        var group = new GroupFake(2).Generate();

        // Act & Assert
        foreach (var chargeStation in group.ChargeStations.ToList())
        {
            group.RemoveChargeStation(chargeStation.Id);
        }

        group.ChargeStations.Count.ShouldBe(0);
    }

    [Fact]
    public void UpdateConnectorCurrent_WithValidData_ShouldSucceed()
    {
        // Arrange
        var group = new GroupFake(numberOfConnectorsPerStation: 3, groupCapacity: 500).Generate();
        var station = group.ChargeStations.First();
        var connector = station.Connectors.First();

        // Act
        group.UpdateConnectorCurrentInAmps(station.Id, connector.Id, CurrentInAmps.Of(20));

        // Assert
        connector.MaxCurrentInAmps.Value.ShouldBe(20);
    }

    [Fact]
    public void UpdateConnectorCurrent_ExceedingCapacity_ShouldThrowDomainException()
    {
        // Arrange
        var groupCapacity = 100; // Explicitly set group capacity
        var connectorCurrent = 30; // Explicitly set max current for connectors
        var group = new GroupFake(
            numberOfConnectorsPerStation: 3,
            groupCapacity: groupCapacity,
            maxConnectorCurrent: connectorCurrent
        ).Generate();

        var station = group.ChargeStations.First();
        var connector = station.Connectors.First();

        var newCurrent = CurrentInAmps.Of(50); // Exceed remaining capacity

        // Act & Assert
        var ex = Should.Throw<DomainException>(() =>
            group.UpdateConnectorCurrentInAmps(station.Id, connector.Id, newCurrent)
        );
        ex.Message.ShouldContain("Updating this connector would exceed group capacity");
    }

    [Fact]
    public void UpdateConnectorCurrent_ExactRemainingCapacity_ShouldSucceed()
    {
        // Arrange
        var groupCapacity = 100;
        var connectorCurrent = 30;
        var group = new GroupFake(
            numberOfConnectorsPerStation: 3,
            groupCapacity: groupCapacity,
            maxConnectorCurrent: connectorCurrent
        ).Generate();

        var station = group.ChargeStations.First();
        var connector = station.Connectors.First();

        // Update to exact remaining capacity
        var newCurrent = CurrentInAmps.Of(40); // 30 -> 40, within remaining capacity

        // Act
        group.UpdateConnectorCurrentInAmps(station.Id, connector.Id, newCurrent);

        // Assert
        connector.MaxCurrentInAmps.Value.ShouldBe(40);
    }

    [Fact]
    public void AddConnectors_ToStation_ShouldSucceed()
    {
        // Arrange
        // Create a group with just 1 connector per station to ensure control over initial setup
        var group = new GroupFake(numberOfConnectorsPerStation: 1, groupCapacity: 100).Generate();
        var station = group.ChargeStations.First();

        // Verify the initial number of connectors for clarity
        station.Connectors.Count.ShouldBe(1);

        // Define two new connectors to be added
        var newConnectors = new[]
        {
            Connector.Create(ConnectorId.Of(2), CurrentInAmps.Of(20)),
            Connector.Create(ConnectorId.Of(3), CurrentInAmps.Of(10)),
        };

        // Act
        group.AddConnectors(station.Id, newConnectors);

        // Assert
        // There should now be 3 connectors in total: 1 initial + 2 new
        station.Connectors.Count.ShouldBe(3);

        // Verify the maximum allowable total (should not exceed group's capacity)
        station.GetTotalCurrent().ShouldBeLessThanOrEqualTo(group.CapacityInAmps.Value);
    }

    [Fact]
    public void AddConnectors_UpToCapacity_ShouldSucceed()
    {
        // Arrange: Create a group with 3 connectors, all using exactly 10 amps
        var group = new GroupFake(
            numberOfConnectorsPerStation: 3,
            groupCapacity: 90,
            maxConnectorCurrent: 10
        ).Generate();
        var station = group.ChargeStations.First();

        // Verify the starting total is as expected (10 amps each, for a total of 30 amps)
        station.GetTotalCurrent().ShouldBe(30); // Current total = 30 amps

        // Define two new connectors, each requiring 30 amps
        var additionalConnectors = new[]
        {
            Connector.Create(ConnectorId.Of(4), CurrentInAmps.Of(30)),
            Connector.Create(ConnectorId.Of(5), CurrentInAmps.Of(30)),
        };

        // Act: Add new connectors
        group.AddConnectors(station.Id, additionalConnectors);

        // Assert: Verify the total current and connector count
        station.Connectors.Count.ShouldBe(5); // Total: 3 existing + 2 new = 5 connectors
        station.GetTotalCurrent().ShouldBe(90); // Total = 30 (existing) + 60 (new) = 90 amps
        group.CapacityInAmps.Value.ShouldBeGreaterThanOrEqualTo(station.GetTotalCurrent()); // Capacity constraint is met
    }

    [Fact]
    public void AddConnectors_ExceedingCapacity_ShouldThrowDomainException()
    {
        // Arrange: Create a group with 3 connectors, all using exactly 10 amps
        var group = new GroupFake(
            numberOfConnectorsPerStation: 3,
            groupCapacity: 90,
            maxConnectorCurrent: 10
        ).Generate();
        var station = group.ChargeStations.First();

        // Define connectors that would exceed the group's capacity
        var additionalConnectors = new[]
        {
            Connector.Create(ConnectorId.Of(4), CurrentInAmps.Of(30)),
            Connector.Create(ConnectorId.Of(5), CurrentInAmps.Of(40)), // Exceeds capacity
        };

        // Act & Assert: Adding these connectors should throw a DomainException
        var ex = Should.Throw<DomainException>(() => group.AddConnectors(station.Id, additionalConnectors));

        ex.Message.ShouldContain("Adding these connectors would exceed group's capacity");
    }

    [Fact]
    public void AddPartialConnectors_ShouldNotExceedCapacity_ShouldSucceed()
    {
        // Arrange
        var groupCapacity = 500;
        var group = new GroupFake(numberOfConnectorsPerStation: 2, groupCapacity: groupCapacity).Generate();
        var station = group.ChargeStations.First();
        var newConnectors = new[] { Connector.Create(ConnectorId.Of(3), CurrentInAmps.Of(20)) };

        // Act
        group.AddConnectors(station.Id, newConnectors);

        // Assert
        station.Connectors.Count.ShouldBe(3); // 2 existing + 1 new
        group.CapacityInAmps.Value.ShouldBeGreaterThanOrEqualTo(station.GetTotalCurrent());
    }

    [Fact]
    public void RemoveConnector_FromNonExistentConnector_ShouldThrowDomainException()
    {
        // Arrange
        var group = new GroupFake(numberOfConnectorsPerStation: 1).Generate(); // A group with 1 connector per station
        var chargeStation = group.ChargeStations.First(); // Get the first charge station

        var nonExistentConnectorId = ConnectorId.Of(3); // A non-existent connector ID

        // Act & Assert
        Should
            .Throw<DomainException>(() => group.RemoveConnectors(chargeStation.Id, new[] { nonExistentConnectorId }))
            .Message.ShouldContain("The following connectors do not exist");
    }

    [Fact]
    public void RemoveAllConnectors_ShouldThrowDomainException()
    {
        // Station should have at least one, but not more than 5 connectors
        // Arrange
        var group = new GroupFake(numberOfConnectorsPerStation: 2).Generate(); // Group with 2 connectors per station
        var chargeStation = group.ChargeStations.First(); // Get the first charge station
        var connectorIds = chargeStation.Connectors.Select(c => c.Id).ToArray(); // Get all connector IDs

        // Act & Assert
        Should
            .Throw<DomainException>(() => group.RemoveConnectors(chargeStation.Id, connectorIds))
            .Message.ShouldContain("Removing these connectors would leave the charge station without any connectors");
    }

    [Fact]
    public void AddDuplicateChargeStation_ShouldThrowDomainException()
    {
        // Arrange
        var group = new GroupFake(1).Generate();
        var chargeStation = group.ChargeStations.First();

        // Act & Assert
        Should
            .Throw<DomainException>(() => group.AddChargeStation(chargeStation))
            .Message.ShouldBe("Charge station with this ID already exists in the group");
    }

    [Fact]
    public void UpdateNonexistentChargeStationName_ShouldThrowDomainException()
    {
        // Arrange
        var group = new GroupFake().Generate();
        var nonExistentStationId = ChargeStationId.New();
        var newName = Name.Of("Non-existent Station");

        // Act & Assert
        Should
            .Throw<DomainException>(() => group.UpdateChargeStationName(nonExistentStationId, newName))
            .Message.ShouldBe("Charge station not found");
    }

    [Fact]
    public void AddConnectors_ExceedingChargeStationLimit_ShouldThrowDomainException()
    {
        // Arrange
        var group = new GroupFake(numberOfConnectorsPerStation: 5, groupCapacity: 500).Generate();
        var station = group.ChargeStations.First();
        var extraConnectors = new[] { Connector.Create(ConnectorId.Of(5), CurrentInAmps.Of(15)) };

        // Act & Assert
        Should
            .Throw<DomainException>(() => group.AddConnectors(station.Id, extraConnectors))
            .Message.ShouldContain("Cannot have more than 5 connectors in a charge station");
    }

    [Fact]
    public void UpdateGroupCapacity_WithNegativeValue_ShouldThrowValidationException()
    {
        // Arrange
        var group = new GroupFake().Generate();

        // Act & Assert
        Should
            .Throw<ValidationException>(() => group.UpdateCapacity(CurrentInAmps.Of(-50)))
            .Message.ShouldContain("Current `-50` must be greater than 0");
    }

    [Fact]
    public void AddSingleChargeStation_ShouldSucceed()
    {
        // Arrange
        var group = new GroupFake().Generate();
        var chargeStation = new ChargeStationFake(numberOfConnectors: 2).Generate();

        // Act
        group.AddChargeStation(chargeStation);

        // Assert
        group.ChargeStations.ShouldContain(chargeStation);
        // Initial station exists, so count should be 2
        group.ChargeStations.Count.ShouldBe(2);
    }

    [Fact]
    public void RemoveSingleChargeStation_ShouldSucceed()
    {
        // Arrange
        var group = new GroupFake().Generate();
        var chargeStationToRemove = group.ChargeStations.First();

        // Act
        group.RemoveChargeStation(chargeStationToRemove.Id);

        // Assert
        group.ChargeStations.ShouldNotContain(chargeStationToRemove);
        group.ChargeStations.Count.ShouldBe(0);
    }
}
