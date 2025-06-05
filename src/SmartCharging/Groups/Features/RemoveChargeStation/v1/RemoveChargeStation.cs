using SmartCharging.Groups.Contracts;
using SmartCharging.Groups.Models;
using SmartCharging.Shared.Application.Contratcs;
using SmartCharging.Shared.Application.Data;
using SmartCharging.Shared.BuildingBlocks.Exceptions;
using SmartCharging.Shared.BuildingBlocks.Extensions;

namespace SmartCharging.Groups.Features.RemoveChargeStation.v1;

public record RemoveChargeStation(Guid GroupId, Guid ChargeStationId)
{
    public static RemoveChargeStation Of(Guid? groupId, Guid? chargeStationId)
    {
        groupId.NotBeNull().NotBeEmpty();
        chargeStationId.NotBeNull().NotBeEmpty();

        return new RemoveChargeStation(groupId.Value, chargeStationId.Value);
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
            throw new NotFoundException($"Group with ID {removeChargeStation.GroupId} not found.");
        }

        RemoveChargeStation(group, removeChargeStation.ChargeStationId);

        unitOfWork.GroupRepository.Update(group);
        await unitOfWork.CommitAsync(cancellationToken);

        logger.LogInformation(
            "Charge station {ChargeStationId} removed from group {GroupId}.",
            removeChargeStation.ChargeStationId,
            removeChargeStation.GroupId
        );
    }

    private static void RemoveChargeStation(Group group, Guid stationId)
    {
        stationId.NotBeNull();

        var station = group.ChargeStations.FirstOrDefault(s => s.Id == stationId);
        if (station == null)
            throw new DomainException("Charge station not found in this group");

        group.ChargeStations.Remove(station);
    }
}
