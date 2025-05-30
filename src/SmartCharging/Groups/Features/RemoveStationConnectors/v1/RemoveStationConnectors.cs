using SmartCharging.Groups.Contracts;
using SmartCharging.Groups.Models.ValueObjects;
using SmartCharging.Shared.BuildingBlocks.Exceptions;
using SmartCharging.Shared.BuildingBlocks.Extensions;

namespace SmartCharging.Groups.Features.RemoveStationConnectors.v1;

public record RemoveStationConnectors(
    GroupId GroupId,
    ChargeStationId ChargeStationId,
    IEnumerable<ConnectorId> ConnectorIds
)
{
    public static RemoveStationConnectors Of(
        Guid? groupId,
        Guid? chargeStationId,
        RemoveStationConnectorsRequest? request
    )
    {
        groupId.NotBeNull();
        chargeStationId.NotBeNull();
        request.NotBeNull();
        request.ConnectorIds.NotBeNull();

        return new RemoveStationConnectors(
            GroupId.Of(groupId),
            ChargeStationId.Of(chargeStationId),
            request.ConnectorIds.Select(ConnectorId.Of).ToList()
        );
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
            throw new NotFoundException($"Group with ID {removeStationConnectors.GroupId.Value} not found.");
        }

        group.RemoveConnectors(removeStationConnectors.ChargeStationId, removeStationConnectors.ConnectorIds.ToList());

        unitOfWork.GroupRepository.Update(group);
        await unitOfWork.CommitAsync(cancellationToken);

        logger.LogInformation(
            "Connectors successfully removed from ChargeStation {ChargeStationId} in Group {GroupId}. Removed Connectors: {ConnectorIds}",
            removeStationConnectors.ChargeStationId.Value,
            removeStationConnectors.GroupId.Value,
            string.Join(", ", removeStationConnectors.ConnectorIds.Select(id => id.Value))
        );
    }
}
