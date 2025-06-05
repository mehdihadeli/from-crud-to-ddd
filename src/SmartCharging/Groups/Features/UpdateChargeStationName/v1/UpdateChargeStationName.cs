using SmartCharging.Groups.Contracts;
using SmartCharging.Groups.Models;
using SmartCharging.Shared.Application.Contratcs;
using SmartCharging.Shared.Application.Data;
using SmartCharging.Shared.BuildingBlocks.Exceptions;
using SmartCharging.Shared.BuildingBlocks.Extensions;

namespace SmartCharging.Groups.Features.UpdateChargeStationName.v1;

public record UpdateChargeStationName(Guid GroupId, Guid ChargeStationId, string NewName)
{
    public static UpdateChargeStationName Of(Guid? groupId, Guid? chargeStationId, string? newName)
    {
        groupId.NotBeNull().NotBeEmpty();
        chargeStationId.NotBeNull().NotBeEmpty();
        newName.NotBeNullOrWhiteSpace();

        return new UpdateChargeStationName(groupId.Value, chargeStationId.Value, newName);
    }
}

public class UpdateChargeStationNameHandler(IUnitOfWork unitOfWork, ILogger<UpdateChargeStationNameHandler> logger)
{
    public async Task Handle(UpdateChargeStationName updateChargeStationName, CancellationToken cancellationToken)
    {
        updateChargeStationName.NotBeNull();

        var group = await unitOfWork.GroupRepository.GetByIdAsync(updateChargeStationName.GroupId, cancellationToken);
        if (group is null)
        {
            throw new NotFoundException($"Group with ID {updateChargeStationName.GroupId} not found.");
        }

        // Update the charge station name
        UpdateChargeStationName(group, updateChargeStationName.ChargeStationId, updateChargeStationName.NewName);

        unitOfWork.GroupRepository.Update(group);
        await unitOfWork.CommitAsync(cancellationToken);

        logger.LogInformation(
            "Charge station {ChargeStationId} name updated to '{NewName}' in group {GroupId}.",
            updateChargeStationName.ChargeStationId,
            updateChargeStationName.NewName,
            updateChargeStationName.GroupId
        );
    }

    private static void UpdateChargeStationName(Group group, Guid chargeStationId, string newName)
    {
        var station = group.ChargeStations.FirstOrDefault(s => s.Id == chargeStationId);
        if (station == null)
            throw new DomainException("Charge station not found");

        station.Name = newName;
    }
}
