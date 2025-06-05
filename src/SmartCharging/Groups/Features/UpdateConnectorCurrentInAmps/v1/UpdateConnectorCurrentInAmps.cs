using SmartCharging.Groups.Contracts;
using SmartCharging.Groups.Models;
using SmartCharging.Shared.Application.Contratcs;
using SmartCharging.Shared.Application.Data;
using SmartCharging.Shared.BuildingBlocks.Exceptions;
using SmartCharging.Shared.BuildingBlocks.Extensions;

namespace SmartCharging.Groups.Features.UpdateConnectorCurrentInAmps.v1;

public record UpdateConnectorCurrentInAmps(Guid GroupId, Guid ChargeStationId, int ConnectorId, int NewCurrentInAmps)
{
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
            throw new NotFoundException($"Group with ID {updateConnectorCurrentInAmps.GroupId} not found.");
        }

        UpdateConnectorCurrentInAmps(
            group,
            updateConnectorCurrentInAmps.ChargeStationId,
            updateConnectorCurrentInAmps.ConnectorId,
            updateConnectorCurrentInAmps.NewCurrentInAmps
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

    private static void UpdateConnectorCurrentInAmps(Group group, Guid stationId, int connectorId, int newCurrent)
    {
        // Input validation (unchanged)
        stationId.NotBeNull();
        connectorId.NotBeNull();
        newCurrent.NotBeNull();

        var station = group.ChargeStations.FirstOrDefault(s => s.Id == stationId);
        if (station == null)
            throw new DomainException("Charge station not found");

        var connector = station.Connectors.FirstOrDefault(c => c.Id == connectorId);
        if (connector == null)
            throw new DomainException("Connector not found");

        // Calculate what the new total would be
        int newTotal = group.GetTotalCurrent() + newCurrent - connector.MaxCurrentInAmps;

        // Validate before making any changes
        if (newTotal > group.CapacityInAmps)
        {
            throw new DomainException(
                $"Updating this connector would exceed group capacity by {newTotal - group.CapacityInAmps} amps"
            );
        }

        connector.MaxCurrentInAmps = newCurrent;
    }
}
