using SmartCharging.Groups.Contracts;
using SmartCharging.Groups.Models.ValueObjects;
using SmartCharging.Shared.BuildingBlocks.Exceptions;
using SmartCharging.Shared.BuildingBlocks.Extensions;

namespace SmartCharging.Groups.Features.UpdateConnectorCurrentInAmps.v1;

public record UpdateConnectorCurrentInAmps(
    GroupId GroupId,
    ChargeStationId ChargeStationId,
    ConnectorId ConnectorId,
    CurrentInAmps NewCurrentInAmps
)
{
    public static UpdateConnectorCurrentInAmps Of(
        Guid? groupId,
        Guid? chargeStationId,
        int connectorId,
        int newCurrentInAmps
    )
    {
        groupId.NotBeNull();
        chargeStationId.NotBeNull();
        connectorId.NotBeNull();
        newCurrentInAmps.NotBeNull();

        return new UpdateConnectorCurrentInAmps(
            GroupId.Of(groupId),
            ChargeStationId.Of(chargeStationId),
            ConnectorId.Of(connectorId),
            CurrentInAmps.Of(newCurrentInAmps)
        );
    }
}

public class UpdateConnectorCurrentInAmpsHandler(
    IUnitOfWork unitOfWork,
    ILogger<UpdateConnectorCurrentInAmpsHandler> logger
)
{
    public async Task Handle(
        UpdateConnectorCurrentInAmps updateConnectorCurrentInAmps,
        CancellationToken cancellationToken
    )
    {
        updateConnectorCurrentInAmps.NotBeNull();

        var group = await unitOfWork.GroupRepository.GetByIdAsync(
            updateConnectorCurrentInAmps.GroupId,
            cancellationToken
        );
        if (group == null)
        {
            throw new NotFoundException($"Group with ID {updateConnectorCurrentInAmps.GroupId.Value} not found.");
        }

        group.UpdateConnectorCurrentInAmps(
            updateConnectorCurrentInAmps.ChargeStationId,
            updateConnectorCurrentInAmps.ConnectorId,
            updateConnectorCurrentInAmps.NewCurrentInAmps
        );

        unitOfWork.GroupRepository.Update(group);
        await unitOfWork.CommitAsync(cancellationToken);

        logger.LogInformation(
            "Successfully updated Connector {ConnectorId} in ChargeStation {ChargeStationId} in Group {GroupId} to {NewCurrentInAmps} amps.",
            updateConnectorCurrentInAmps.ConnectorId.Value,
            updateConnectorCurrentInAmps.ChargeStationId.Value,
            updateConnectorCurrentInAmps.GroupId.Value,
            updateConnectorCurrentInAmps.NewCurrentInAmps.Value
        );
    }
}
