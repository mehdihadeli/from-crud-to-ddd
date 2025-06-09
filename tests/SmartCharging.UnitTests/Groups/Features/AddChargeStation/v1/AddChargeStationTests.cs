using SmartCharging.Groups.Models;
using SmartCharging.Groups.Models.ValueObjects;
using SmartCharging.Shared.BuildingBlocks.Exceptions;

namespace SmartCharging.UnitTests.Groups.Features.AddChargeStation.v1;

public class AddChargeStationTests
{
    [Fact]
    public void AddChargeStation_WithValidData_AddsStationToGroupSuccessfully()
    {
        // Arrange: Create a group with capacity and no initial stations
        var groupName = Name.Of("Group A");
        var groupCapacityInAmps = CurrentInAmps.Of(500);
        var group = Group.Create(GroupId.New(), groupName, groupCapacityInAmps, chargeStation: null);

        // Create a valid new charge station
        var chargeStationId = ChargeStationId.New();
        var chargeStationName = Name.Of("Station A");
        var connectors = new List<Connector>
        {
            Connector.Create(ConnectorId.Of(1), CurrentInAmps.Of(50)),
            Connector.Create(ConnectorId.Of(2), CurrentInAmps.Of(100)),
        }.AsReadOnly();

        var chargeStation = ChargeStation.Create(chargeStationId, chargeStationName, connectors);

        // Act: Add the charge station to the group
        group.AddChargeStation(chargeStation);

        // Assert
        group.ChargeStations.ShouldContain(station => station.Id == chargeStation.Id);
    }

    [Fact]
    public void AddChargeStation_WithExceedingCurrent_ThrowsDomainException()
    {
        // Arrange: Create a group with a restricted capacity
        var groupName = Name.Of("Restricted Group");
        var limitedGroupCapacity = CurrentInAmps.Of(100);
        var group = Group.Create(GroupId.New(), groupName, limitedGroupCapacity, chargeStation: null);

        // Create a charge station that exceeds group capacity
        var chargeStationId = ChargeStationId.New();
        var chargeStationName = Name.Of("Station Exceeding");
        var connectors = new List<Connector>
        {
            Connector.Create(ConnectorId.Of(1), CurrentInAmps.Of(70)),
            Connector.Create(ConnectorId.Of(2), CurrentInAmps.Of(50)), // Total 120 amps
        }.AsReadOnly();

        var chargeStation = ChargeStation.Create(chargeStationId, chargeStationName, connectors);

        // Act & Assert: Adding the charge station should throw a DomainException
        var exception = Should.Throw<DomainException>(() => group.AddChargeStation(chargeStation));
        exception.Message.ShouldBe("Adding this station would exceed group capacity");
    }

    [Fact]
    public void AddChargeStation_WithDuplicateStation_ThrowsDomainException()
    {
        // Arrange: Create a group with one existing charge station
        var groupName = Name.Of("Duplicate Group");
        var groupCapacityInAmps = CurrentInAmps.Of(500);
        var chargeStationId = ChargeStationId.New();
        var existingStation = ChargeStation.Create(
            chargeStationId,
            Name.Of("Station 1"),
            new List<Connector> { Connector.Create(ConnectorId.Of(1), CurrentInAmps.Of(100)) }.AsReadOnly()
        );
        var group = Group.Create(GroupId.New(), groupName, groupCapacityInAmps, existingStation);

        // Create a new charge station with the same ID as the existing one
        var duplicateStation = ChargeStation.Create(
            chargeStationId,
            Name.Of("Duplicate"),
            new List<Connector> { Connector.Create(ConnectorId.Of(2), CurrentInAmps.Of(50)) }.AsReadOnly()
        );

        // Act & Assert: Adding the duplicate station should throw a DomainException
        var exception = Should.Throw<DomainException>(() => group.AddChargeStation(duplicateStation));
        exception.Message.ShouldBe("Charge station with this ID already exists in the group");
    }

    [Fact]
    public void AddChargeStation_WithLessThanOneConnector_ThrowsDomainException()
    {
        // Arrange: Create an empty group
        var groupName = Name.Of("Empty Group");
        var groupCapacityInAmps = CurrentInAmps.Of(300);
        var group = Group.Create(GroupId.New(), groupName, groupCapacityInAmps, chargeStation: null);

        // Create a charge station with no connectors
        var stationWithNoConnectors = ChargeStation.Create(
            ChargeStationId.New(),
            Name.Of("No Connectors Station"),
            new List<Connector>().AsReadOnly()
        );

        // Act & Assert: Attempting to add a station with no connectors should fail
        var exception = Should.Throw<DomainException>(() => group.AddChargeStation(stationWithNoConnectors));
        exception.Message.ShouldBe("Charge station must have at least one connector");
    }

    [Fact]
    public void AddChargeStation_WithInvalidName_ShouldThrowValidationException()
    {
        // Arrange: Prepare an invalid name
        var invalidName = default(Name); // Null name
        var groupCapacityInAmps = CurrentInAmps.Of(500);
        var invalidGroup = Group.Create(GroupId.New(), Name.Of("Valid Group"), groupCapacityInAmps, null);

        // Create a charge station with the invalid name
        var invalidStation = new Func<ChargeStation>(() =>
            ChargeStation.Create(
                ChargeStationId.New(),
                invalidName!,
                new List<Connector> { Connector.Create(ConnectorId.Of(1), CurrentInAmps.Of(100)) }.AsReadOnly()
            )
        );

        // Act & Assert: Attempting to create the station should throw ValidationException
        Should.Throw<ValidationException>(invalidStation);
    }
}
