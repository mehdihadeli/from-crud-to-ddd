using Bogus;
using SmartCharging.Groups.Dtos;
using SmartCharging.Groups.Features.CreateGroup.v1;

namespace SmartCharging.EndToEndTests.Group.Mocks;

public sealed class CreateGroupRequestFake : Faker<CreateGroupRequest>
{
    public CreateGroupRequestFake(int numberOfConnectors = 3)
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
                    ChargeStationId: Guid.NewGuid(), // Placeholder since these are yet to be associated with the final charge station
                    ConnectorId: connectorId,
                    MaxCurrentInAmps: faker.Random.Int(10, 50)
                ))
                .ToList();

            // Generate the charge station with its connectors
            var chargeStation = new ChargeStationDto(
                ChargeStationId: Guid.NewGuid(),
                Name: faker.Company.CompanyName(),
                Connectors: connectors.AsReadOnly()
            );

            // Generate the CreateGroupRequest with the charge station
            return new CreateGroupRequest(
                Name: faker.Commerce.Department(),
                CapacityInAmps: Math.Max(100, connectors.Sum(c => c.MaxCurrentInAmps) + faker.Random.Int(10, 100)),
                ChargeStation: chargeStation
            );
        });
    }
}
