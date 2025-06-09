using SmartCharging.Groups.Models;
using SmartCharging.Groups.Models.ValueObjects;
using SmartCharging.Shared.BuildingBlocks.Exceptions;
using SmartCharging.UnitTests.Groups.Mocks;

namespace SmartCharging.UnitTests.Groups.Features.CreateGroup.v1;

public class CreateGroupTests
{
    [Fact]
    public void CreateGroup_With_ValidData_CreatesGroupSuccessfully()
    {
        // Arrange
        var groupName = Name.Of("Group A");
        var capacityInAmps = CurrentInAmps.Of(500);
        var chargeStation = new ChargeStationFake(3).Generate();

        // Act
        var group = Group.Create(GroupId.New(), groupName, capacityInAmps, chargeStation);

        // Assert
        group.ShouldNotBeNull();
        group.Name.ShouldBe(groupName);
        group.CapacityInAmps.ShouldBe(capacityInAmps);
        group.ChargeStations.ShouldContain(chargeStation);
    }

    [Fact]
    public void AddChargeStation_With_ExceedingCurrent_ThrowsDomainException()
    {
        // Arrange
        var group = new GroupFake(numberOfConnectorsPerStation: 1, 100, 60).Generate();

        // Create a ChargeStation where the total connector current exceeds group capacity
        var chargeStation = new ChargeStationFake(numberOfConnectors: 3)
            .RuleFor(
                cs => cs.Connectors,
                faker => new List<Connector>
                {
                    Connector.Create(ConnectorId.Of(1), CurrentInAmps.Of(60)),
                    Connector.Create(ConnectorId.Of(2), CurrentInAmps.Of(50)), // This exceeds capacity
                }
            )
            .Generate();

        // Act & Assert: Ensure attempting to add this station to the group throws an exception
        var exception = Should.Throw<DomainException>(() => group.AddChargeStation(chargeStation));
        exception.Message.ShouldBe("Adding this station would exceed group capacity");
    }

    [Fact]
    public void CreateGroup_With_InvalidName_ThrowsValidationException()
    {
        // Arrange
        var invalidName = default(Name); // Invalid name
        var capacityInAmps = CurrentInAmps.Of(500);
        var chargeStation = new ChargeStationFake(3).Generate();

        // Act & Assert
        Should.Throw<ValidationException>(() => Group.Create(GroupId.New(), invalidName!, capacityInAmps, chargeStation)
        );
    }
}
