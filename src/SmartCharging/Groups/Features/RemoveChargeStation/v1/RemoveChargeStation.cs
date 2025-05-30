using SmartCharging.Groups.Contracts;
using SmartCharging.Groups.Models.ValueObjects;
using SmartCharging.Shared.BuildingBlocks.Exceptions;
using SmartCharging.Shared.BuildingBlocks.Extensions;

namespace SmartCharging.Groups.Features.RemoveChargeStation.v1;

public record RemoveChargeStation(GroupId GroupId, ChargeStationId ChargeStationId)
{
    public static RemoveChargeStation Of(Guid? groupId, Guid? chargeStationId)
    {
        groupId.NotBeNull();
        chargeStationId.NotBeNull();

        return new RemoveChargeStation(GroupId.Of(groupId.Value), ChargeStationId.Of(chargeStationId.Value));
    }
}

public class RemoveChargeStationHandler(IUnitOfWork unitOfWork, ILogger<RemoveChargeStationHandler> logger)
{
    public async Task Handle(RemoveChargeStation removeChargeStation, CancellationToken cancellationToken)
    {
        removeChargeStation.NotBeNull();

        var group = await unitOfWork.GroupRepository.GetByIdAsync(removeChargeStation.GroupId, cancellationToken);
        if (group is null)
        {
            throw new NotFoundException($"Group with ID {removeChargeStation.GroupId.Value} not found.");
        }

        group.RemoveChargeStation(removeChargeStation.ChargeStationId);

        unitOfWork.GroupRepository.Update(group);
        await unitOfWork.CommitAsync(cancellationToken);

        logger.LogInformation(
            "Charge station {ChargeStationId} removed from group {GroupId}.",
            removeChargeStation.ChargeStationId.Value,
            removeChargeStation.GroupId.Value
        );
    }
}
