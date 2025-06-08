using SmartCharging.Groups.Models.ValueObjects;
using SmartCharging.Shared.Application.Contracts;
using SmartCharging.Shared.Application.Data;
using SmartCharging.Shared.BuildingBlocks.Exceptions;
using SmartCharging.Shared.BuildingBlocks.Extensions;

namespace SmartCharging.Groups.Features.RemoveChargeStation.v1;

public sealed record RemoveChargeStation(Guid GroupId, Guid ChargeStationId)
{
    // - just input validation inside command static constructor and business rules or domain-level validation to the command-handler and construct the Value Objects/Entities within the command-handler
    public static RemoveChargeStation Of(Guid? groupId, Guid? chargeStationId)
    {
        groupId.NotBeNull().NotBeEmpty();
        chargeStationId.NotBeNull().NotBeEmpty();

        return new RemoveChargeStation(groupId.Value, chargeStationId.Value);
    }
}

public sealed class RemoveChargeStationHandler(IUnitOfWork unitOfWork, ILogger<RemoveChargeStationHandler> logger)
{
    public async Task Handle(RemoveChargeStation removeChargeStation, CancellationToken cancellationToken)
    {
        removeChargeStation.NotBeNull();

        // Business rules validation in value objects and entities will do in handlers, not commands, and in command we just have input validations
        var group = await unitOfWork.GroupRepository.GetByIdAsync(
            GroupId.Of(removeChargeStation.GroupId),
            cancellationToken
        );
        if (group is null)
        {
            throw new NotFoundException($"Group with ID {removeChargeStation.GroupId} not found.");
        }

        group.RemoveChargeStation(ChargeStationId.Of(removeChargeStation.ChargeStationId));

        unitOfWork.GroupRepository.Update(group);
        await unitOfWork.CommitAsync(cancellationToken);

        logger.LogInformation(
            "Charge station {ChargeStationId} removed from group {GroupId}.",
            removeChargeStation.ChargeStationId,
            removeChargeStation.GroupId
        );
    }
}
