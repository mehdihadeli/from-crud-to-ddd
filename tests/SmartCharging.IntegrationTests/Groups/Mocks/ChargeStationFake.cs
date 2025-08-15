using Bogus;
using SmartChargingApi.Groups.Models;
using SmartChargingApi.Groups.Models.ValueObjects;

namespace SmartCharging.IntegrationTests.Groups.Mocks;

/// <summary>
/// Creates a fake ChargeStation instance adhering to domain rules.
/// </summary>
public sealed class ChargeStationFake : Faker<ChargeStation>
{
    public ChargeStationFake(int numberOfConnectors)
    {
        if (numberOfConnectors is < 1 or > 5)
            throw new ArgumentOutOfRangeException(
                nameof(numberOfConnectors),
                "A ChargeStation must have between 1 and 5 Connectors."
            );

        CustomInstantiator(faker =>
        {
            // Generate Connectors with random MaxCurrentInAmps
            var chargeStationId = ChargeStationId.New();
            var connectors = Enumerable
                .Range(1, numberOfConnectors)
                .Select(connectorIndex =>
                {
                    var maxCurrent = faker.Random.Int(10, 50);
                    return Connector.Create(ConnectorId.Of(connectorIndex), CurrentInAmps.Of(maxCurrent));
                })
                .ToList();

            return ChargeStation.Create(chargeStationId, Name.Of(faker.Commerce.ProductName()), connectors);
        });
    }
}
