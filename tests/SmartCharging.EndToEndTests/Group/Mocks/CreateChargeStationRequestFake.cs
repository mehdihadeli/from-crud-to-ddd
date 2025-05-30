using Bogus;
using SmartCharging.Groups.Dtos;
using SmartCharging.Groups.Features.AddChargeStation.v1;

namespace SmartCharging.EndToEndTests.Group.Mocks;

public sealed class CreateChargeStationRequestFake : Faker<AddChargeStationRequest>
{
    public CreateChargeStationRequestFake(Guid groupId, int numberOfConnectors = 3)
    {
        if (numberOfConnectors is < 1 or > 5)
            throw new ArgumentOutOfRangeException(
                nameof(numberOfConnectors),
                "A ChargeStation must have between 1 and 5 connectors."
            );

        CustomInstantiator(faker =>
        {
            // Generate a list of connectors
            var connectors = Enumerable
                .Range(1, numberOfConnectors)
                .Select(connectorId => new ConnectorDto(
                    ChargeStationId: Guid.NewGuid(),
                    ConnectorId: connectorId,
                    MaxCurrentInAmps: faker.Random.Int(10, 50)
                ))
                .ToList();

            // Generate the AddChargeStation request with the specified group and connectors
            return new AddChargeStationRequest(
                ChargeStationId: Guid.NewGuid(),
                Name: faker.Company.CompanyName(),
                Connectors: connectors.AsReadOnly()
            );
        });
    }
}
