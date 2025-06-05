using SmartCharging.Groups.Dtos;
using SmartCharging.Groups.Models;
using SmartCharging.Shared.Application.Contratcs;
using SmartCharging.Shared.BuildingBlocks.Exceptions;
using SmartCharging.Shared.BuildingBlocks.Extensions;

namespace SmartCharging.Groups.Features.AddChargeStation.v1;

public record AddChargeStation(Guid GroupId, string Name, IReadOnlyCollection<ConnectorDto> Connectors)
{
    public static AddChargeStation Of(Guid? groupId, string? name, IReadOnlyCollection<ConnectorDto>? connectors)
    {
        groupId.NotBeNull().NotBeEmpty();
        name.NotBeEmptyOrNull();
        connectors.NotBeNull();

        return new AddChargeStation(groupId.Value, name, connectors);
    }

    public Guid ChargeStationId { get; } = Guid.CreateVersion7();
}

public class AddChargeStationHandler(
    IUnitOfWork unitOfWork,
    ILogger<AddChargeStationHandler> logger,
    IBusinessRuleValidator ruleValidator
)
{
    public async Task<Guid> Handle(AddChargeStation addChargeStation, CancellationToken cancellationToken)
    {
        addChargeStation.NotBeNull();

        var group = await unitOfWork.GroupRepository.GetByIdAsync(addChargeStation.GroupId, cancellationToken);
        if (group is null)
        {
            throw new NotFoundException($"Group with ID {addChargeStation.GroupId} not found.");
        }

        var chargeStation = new ChargeStation
        {
            GroupId = addChargeStation.GroupId,
            Name = addChargeStation.Name,
            Id = addChargeStation.ChargeStationId,
            Connectors = addChargeStation.Connectors.ToConnectors(),
        };
        // Add the charge station to the group
        AddChargeStation(chargeStation, group, ruleValidator);

        // Mark the group as updated
        unitOfWork.GroupRepository.Update(group);
        await unitOfWork.CommitAsync(cancellationToken);

        logger.LogInformation(
            "Charge station {ChargeStationId} with {ConnectorCount} connectors added to group {GroupId}.",
            chargeStation.Id,
            chargeStation.Connectors.Count,
            addChargeStation.GroupId
        );

        return addChargeStation.ChargeStationId;
    }

    private static void AddChargeStation(ChargeStation chargeStation, Group group, IBusinessRuleValidator ruleValidator)
    {
        chargeStation.NotBeNull();

        // Rule 1: Validate unique charge station
        ruleValidator.ValidateChargeStationUniqueness(group, chargeStation.Id);

        // Rule 2: Validate connector configuration
        ruleValidator.ValidateConnectorConfiguration(chargeStation.Connectors);

        // Rule 3: Validate group capacity for addition
        ruleValidator.ValidateCapacityForAdditions(group, chargeStation.GetTotalCurrent());

        group.ChargeStations.Add(chargeStation);
    }
}
