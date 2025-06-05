using SmartCharging.Groups.Contracts;
using SmartCharging.Groups.Models;
using SmartCharging.Shared.Application.Contratcs;
using SmartCharging.Shared.Application.Data;
using SmartCharging.Shared.BuildingBlocks.Exceptions;
using SmartCharging.Shared.BuildingBlocks.Extensions;

namespace SmartCharging.Groups.Features.RemoveStationConnectors.v1;

public record RemoveStationConnectors(Guid GroupId, Guid ChargeStationId, IReadOnlyCollection<int> ConnectorIds)
{
    public static RemoveStationConnectors Of(
        Guid? groupId,
        Guid? chargeStationId,
        RemoveStationConnectorsRequest? request
    )
    {
        groupId.NotBeNull().NotBeEmpty();
        chargeStationId.NotBeNull().NotBeEmpty();
        request.NotBeNull();
        request.ConnectorIds.NotBeNull();

        return new RemoveStationConnectors(groupId.Value, chargeStationId.Value, request.ConnectorIds.ToList());
    }
}

public class RemoveStationConnectorsHandler(IUnitOfWork unitOfWork, ILogger<RemoveStationConnectorsHandler> logger)
{
    public async Task Handle(RemoveStationConnectors removeStationConnectors, CancellationToken cancellationToken)
    {
        removeStationConnectors.NotBeNull();

        var group = await unitOfWork.GroupRepository.GetByIdAsync(removeStationConnectors.GroupId, cancellationToken);
        if (group is null)
        {
            throw new NotFoundException($"Group with ID {removeStationConnectors.GroupId} not found.");
        }

        RemoveConnectors(group, removeStationConnectors.ChargeStationId, removeStationConnectors.ConnectorIds.ToList());

        unitOfWork.GroupRepository.Update(group);
        await unitOfWork.CommitAsync(cancellationToken);

        logger.LogInformation(
            "Connectors successfully removed from ChargeStation {ChargeStationId} in Group {GroupId}. Removed Connectors: {ConnectorIds}",
            removeStationConnectors.ChargeStationId,
            removeStationConnectors.GroupId,
            string.Join(", ", removeStationConnectors.ConnectorIds.Select(id => id))
        );
    }

    private static void RemoveConnectors(Group group, Guid stationId, IList<int> connectorIds)
    {
        stationId.NotBeNull();

        var station = group.ChargeStations.FirstOrDefault(s => s.Id == stationId);
        if (station == null)
            throw new DomainException("Charge station not found");

        // Validate that all specified IDs exist in the current connectors
        var existingIds = station.Connectors.Select(c => c.Id).ToHashSet();
        var nonExistentIds = connectorIds.Where(id => !existingIds.Contains(id)).ToList();
        if (nonExistentIds.Any())
            throw new DomainException($"The following connectors do not exist: {string.Join(", ", nonExistentIds)}");

        // Validate that at least one connector will remain after the removal
        if (station.Connectors.Count - connectorIds.Count < 1)
            throw new DomainException(
                "Removing these connectors would leave the charge station without any connectors"
            );

        station.Connectors.ToList().RemoveAll(c => connectorIds.Contains(c.Id));
    }
}
