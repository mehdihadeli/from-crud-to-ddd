using SmartCharging.Groups.Contracts;
using SmartCharging.Groups.Models.ValueObjects;
using SmartCharging.Shared.BuildingBlocks.Exceptions;
using SmartCharging.Shared.BuildingBlocks.Extensions;

namespace SmartCharging.Groups.Features.UpdateChargeStationName.v1;

public record UpdateChargeStationName(GroupId GroupId, ChargeStationId ChargeStationId, Name NewName)
{
    public static UpdateChargeStationName Of(Guid? groupId, Guid? chargeStationId, string? newName)
    {
        groupId.NotBeNull();
        chargeStationId.NotBeNull();
        newName.NotBeNullOrWhiteSpace();

        return new UpdateChargeStationName(
            GroupId.Of(groupId.Value),
            ChargeStationId.Of(chargeStationId.Value),
            Name.Of(newName)
        );
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
            throw new NotFoundException($"Group with ID {updateChargeStationName.GroupId.Value} not found.");
        }

        // Update the charge station name
        group.UpdateChargeStationName(updateChargeStationName.ChargeStationId, updateChargeStationName.NewName);

        unitOfWork.GroupRepository.Update(group);
        await unitOfWork.CommitAsync(cancellationToken);

        // Log the change
        logger.LogInformation(
            "Charge station {ChargeStationId} name updated to '{NewName}' in group {GroupId}.",
            updateChargeStationName.ChargeStationId.Value,
            updateChargeStationName.NewName.Value,
            updateChargeStationName.GroupId.Value
        );
    }
}
