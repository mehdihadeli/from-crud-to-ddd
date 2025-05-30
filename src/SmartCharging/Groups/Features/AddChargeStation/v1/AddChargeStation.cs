using SmartCharging.Groups.Contracts;
using SmartCharging.Groups.Dtos;
using SmartCharging.Groups.Models;
using SmartCharging.Groups.Models.ValueObjects;
using SmartCharging.Shared.BuildingBlocks.Exceptions;
using SmartCharging.Shared.BuildingBlocks.Extensions;

namespace SmartCharging.Groups.Features.AddChargeStation.v1;

public record AddChargeStation(GroupId GroupId, ChargeStation ChargeStation)
{
    public static AddChargeStation Of(
        Guid? groupId,
        Guid? chargeStationId,
        string? name,
        IReadOnlyCollection<ConnectorDto>? connectors
    )
    {
        groupId.NotBeNull();
        chargeStationId.NotBeNull();
        name.NotBeNull();
        connectors.NotBeNull();

        var chargeStation = ChargeStation.Create(
            ChargeStationId.Of(chargeStationId),
            Name.Of(name),
            connectors.ToConnectors()
        );

        return new AddChargeStation(GroupId.Of(groupId.Value), chargeStation);
    }
}

public class AddChargeStationHandler(IUnitOfWork unitOfWork, ILogger<AddChargeStationHandler> logger)
{
    public async Task Handle(AddChargeStation addChargeStation, CancellationToken cancellationToken)
    {
        addChargeStation.NotBeNull();

        var group = await unitOfWork.GroupRepository.GetByIdAsync(addChargeStation.GroupId, cancellationToken);
        if (group is null)
        {
            throw new NotFoundException($"Group with ID {addChargeStation.GroupId.Value} not found.");
        }

        group.AddChargeStation(addChargeStation.ChargeStation);

        // Mark the group as updated
        unitOfWork.GroupRepository.Update(group);
        await unitOfWork.CommitAsync(cancellationToken);

        logger.LogInformation(
            "Charge station {ChargeStationId} with {ConnectorCount} connectors added to group {GroupId}.",
            addChargeStation.ChargeStation.Id.Value,
            addChargeStation.ChargeStation.Connectors.Count,
            addChargeStation.GroupId.Value
        );
    }
}
