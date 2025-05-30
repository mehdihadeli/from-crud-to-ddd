using Bogus;
using SmartCharging.Groups.Models;
using SmartCharging.Groups.Models.ValueObjects;

namespace SmartCharging.IntegrationTests.Groups.Mocks;

/// <summary>
/// Creates a fake `Group` with valid data adhering to domain rules.
/// </summary>
public sealed class GroupFake : Faker<Group>
{
    public GroupFake(int numberOfConnectorsPerStation = 2, int groupCapacity = 500, int? maxConnectorCurrent = null)
    {
        if (numberOfConnectorsPerStation is < 1 or > 5)
            throw new ArgumentOutOfRangeException(
                nameof(numberOfConnectorsPerStation),
                "A ChargeStation must have between 1 and 5 Connectors."
            );

        CustomInstantiator(f =>
        {
            // Explicitly set or randomly generate the group's CapacityInAmps
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
            return Group.Create(
                GroupId.New(),
                Name.Of(f.Commerce.Department()),
                CurrentInAmps.Of(groupCapacity),
                chargeStation
            );
        });
    }
}
