using Bogus;
using SmartCharging.Groups.Models;
using SmartCharging.Groups.Models.ValueObjects;

namespace SmartCharging.EndToEndTests.Group.Mocks;

/// <summary>
/// Creates a fake `Group` with valid data adhering to domain rules.
/// </summary>
public sealed class GroupFake : Faker<Groups.Models.Group>
{
    public GroupFake(int numberOfConnectorsPerStation = 2, int? groupCapacity = null, int? maxConnectorCurrent = null)
    {
        if (numberOfConnectorsPerStation is < 1 or > 5)
            throw new ArgumentOutOfRangeException(
                nameof(numberOfConnectorsPerStation),
                "A ChargeStation must have between 1 and 5 Connectors."
            );

        CustomInstantiator(f =>
        {
            // Explicitly set or randomly generate the group's CapacityInAmps
            var groupCapacityValue = groupCapacity ?? f.Random.Int(100, 500);

            var chargeStationId = ChargeStationId.New();

            // Generate connectors with specified or random MaxCurrentInAmps
            var connectors = Enumerable
                .Range(1, numberOfConnectorsPerStation)
                .Select(connectorId =>
                {
                    var currentAmp = maxConnectorCurrent ?? f.Random.Int(10, 50);
                    return Connector.Create(ConnectorId.Of(connectorId), CurrentInAmps.Of(currentAmp), chargeStationId);
                })
                .ToList();

            var chargeStation = ChargeStation.Create(chargeStationId, Name.Of(f.Commerce.ProductName()), connectors);

            // Create the group with explicit or random CapacityInAmps
            return Groups.Models.Group.Create(
                GroupId.New(),
                Name.Of(f.Commerce.Department()),
                CurrentInAmps.Of(groupCapacityValue),
                chargeStation
            );
        });
    }
}
