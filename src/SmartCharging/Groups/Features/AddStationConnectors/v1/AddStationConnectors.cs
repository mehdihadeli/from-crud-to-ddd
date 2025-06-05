using SmartCharging.Groups.Dtos;
using SmartCharging.Groups.Models;
using SmartCharging.Shared.Application.Contratcs;
using SmartCharging.Shared.BuildingBlocks.Exceptions;
using SmartCharging.Shared.BuildingBlocks.Extensions;

namespace SmartCharging.Groups.Features.AddStationConnectors.v1;

public record AddStationConnectors(Guid GroupId, Guid ChargeStationId, IReadOnlyCollection<ConnectorDto> Connectors)
{
    public static AddStationConnectors Of(
        Guid? groupId,
        Guid? chargeStationId,
        IReadOnlyCollection<ConnectorDto>? connectors
    )
    {
        groupId.NotBeNull().NotBeEmpty();
        chargeStationId.NotBeNull().NotBeEmpty();
        connectors.NotBeNull();

        return new AddStationConnectors(groupId.Value, chargeStationId.Value, connectors);
    }
}

public class AddConnectorsHandler(
    IUnitOfWork unitOfWork,
    ILogger<AddConnectorsHandler> logger,
    IBusinessRuleValidator ruleValidator
)
{
    public async Task<AddStationConnectorsResult> Handle(
        AddStationConnectors addStationConnectors,
        CancellationToken cancellationToken
    )
    {
        // Step 1: Basic validation
        addStationConnectors.NotBeNull();

        // Step 2: Fetch the group
        var group = await unitOfWork.GroupRepository.GetByIdAsync(addStationConnectors.GroupId, cancellationToken);
        if (group is null)
        {
            throw new NotFoundException($"Group with ID {addStationConnectors.GroupId} not found.");
        }

        // Step 3: Add connectors
        AddConnectors(group, addStationConnectors.ChargeStationId, addStationConnectors.Connectors);

        // Step 4: Save changes to a repository
        unitOfWork.GroupRepository.Update(group);
        await unitOfWork.CommitAsync(cancellationToken);

        logger.LogInformation(
            "Connectors successfully added to ChargeStation {ChargeStationId} in Group {GroupId}. Added Connectors: {Connectors}",
            addStationConnectors.ChargeStationId,
            addStationConnectors.GroupId,
            string.Join(", ", addStationConnectors.Connectors.Select(c => c.ConnectorId))
        );

        return new AddStationConnectorsResult(addStationConnectors.Connectors);
    }

    private void AddConnectors(Group group, Guid stationId, IReadOnlyCollection<ConnectorDto> connectorsDto)
    {
        // Validate connectors input
        ruleValidator.ValidateConnectorConfiguration(connectorsDto.ToConnectors());

        // Fetch the specified charge station
        var station = group.ChargeStations.FirstOrDefault(s => s.Id == stationId);
        if (station is null)
        {
            throw new DomainException("Charge station not found.");
        }

        // Validate group capacity for adding connectors
        int additionalLoad = connectorsDto.Sum(c => c.MaxCurrentInAmps);
        ruleValidator.ValidateCapacityForAdditions(group, additionalLoad);

        // Validate the uniqueness of connector IDs within the station
        ruleValidator.ValidateConnectorUniqueness(station.Connectors, connectorsDto);

        // Add new connectors to the charge station
        foreach (var connector in connectorsDto.ToConnectors())
        {
            station.Connectors.Add(connector);
        }
    }
}

public record AddStationConnectorsResult(IReadOnlyCollection<ConnectorDto> Connectors);
