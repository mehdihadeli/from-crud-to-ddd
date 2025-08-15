using SmartCharging.ServiceDefaults.Exceptions;
using SmartCharging.ServiceDefaults.Extensions;
using SmartChargingApi.Groups.Models.ValueObjects;
using SmartChargingApi.Shared.Contracts;

namespace SmartChargingApi.Groups.Features.UpdateConnectorCurrentInAmps.v1;

public sealed record UpdateConnectorCurrentInAmps(
    Guid GroupId,
    Guid ChargeStationId,
    int ConnectorId,
    int NewCurrentInAmps
)
{
    // - just input validation inside command static constructor and business rules or domain-level validation to the command-handler and construct the Value Objects/Entities within the command-handler
    public static UpdateConnectorCurrentInAmps Of(
        Guid? groupId,
        Guid? chargeStationId,
        int connectorId,
        int newCurrentInAmps
    )
    {
        groupId.NotBeNull().NotBeEmpty();
        chargeStationId.NotBeNull().NotBeEmpty();
        connectorId.NotBeNegativeOrZero();
        newCurrentInAmps.NotBeNegativeOrZero();

        return new UpdateConnectorCurrentInAmps(groupId.Value, chargeStationId.Value, connectorId, newCurrentInAmps);
    }
}

public sealed class UpdateConnectorCurrentInAmpsHandler(
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

        // Business rules validation in value objects and entities will do in handlers, not commands, and in command we just have input validations
        var group = await unitOfWork.GroupRepository.GetByIdAsync(
            GroupId.Of(updateConnectorCurrentInAmps.GroupId),
            cancellationToken
        );
        if (group == null)
        {
            throw new NotFoundException($"Group with ID {updateConnectorCurrentInAmps.GroupId} not found.");
        }

        group.UpdateConnectorCurrentInAmps(
            ChargeStationId.Of(updateConnectorCurrentInAmps.ChargeStationId),
            ConnectorId.Of(updateConnectorCurrentInAmps.ConnectorId),
            CurrentInAmps.Of(updateConnectorCurrentInAmps.NewCurrentInAmps)
        );

        unitOfWork.GroupRepository.Update(group);
        await unitOfWork.CommitAsync(cancellationToken);

        logger.LogInformation(
            "Successfully updated Connector {ConnectorId} in ChargeStation {ChargeStationId} in Group {GroupId} to {NewCurrentInAmps} amps.",
            updateConnectorCurrentInAmps.ConnectorId,
            updateConnectorCurrentInAmps.ChargeStationId,
            updateConnectorCurrentInAmps.GroupId,
            updateConnectorCurrentInAmps.NewCurrentInAmps
        );
    }
}
