using SmartCharging.Groups.Models.ValueObjects;
using SmartCharging.Shared.Application.Data;
using SmartCharging.Shared.BuildingBlocks.Exceptions;
using SmartCharging.Shared.BuildingBlocks.Extensions;

namespace SmartCharging.Groups.Features.UpdateChargeStationName.v1;

public record UpdateChargeStationName(Guid GroupId, Guid ChargeStationId, string NewName)
{
    // - just input validation inside command static constructor and business rules or domain-level validation to the command-handler and construct the Value Objects/Entities within the command-handler
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

        // Business rules validation in value objects and entities will do in handlers, not commands, and in command we just have input validations
        var group = await unitOfWork.GroupRepository.GetByIdAsync(
            GroupId.Of(updateChargeStationName.GroupId),
            cancellationToken
        );
        if (group is null)
        {
            throw new NotFoundException($"Group with ID {updateChargeStationName.GroupId} not found.");
        }

        // Update the charge station name
        group.UpdateChargeStationName(
            ChargeStationId.Of(updateChargeStationName.ChargeStationId),
            Name.Of(updateChargeStationName.NewName)
        );

        unitOfWork.GroupRepository.Update(group);
        await unitOfWork.CommitAsync(cancellationToken);

        // Log the change
        logger.LogInformation(
            "Charge station {ChargeStationId} name updated to '{NewName}' in group {GroupId}.",
            updateChargeStationName.ChargeStationId,
            updateChargeStationName.NewName,
            updateChargeStationName.GroupId
        );
    }
}
