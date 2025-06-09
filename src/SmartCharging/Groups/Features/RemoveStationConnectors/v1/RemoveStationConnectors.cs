using SmartCharging.Groups.Models.ValueObjects;
using SmartCharging.Shared.Application.Contracts;
using SmartCharging.Shared.BuildingBlocks.Exceptions;
using SmartCharging.Shared.BuildingBlocks.Extensions;

namespace SmartCharging.Groups.Features.RemoveStationConnectors.v1;

public sealed record RemoveStationConnectors(Guid GroupId, Guid ChargeStationId, IReadOnlyCollection<int> ConnectorIds)
{
    // - just input validation inside command static constructor and business rules or domain-level validation to the command-handler and construct the Value Objects/Entities within the command-handler
    public static RemoveStationConnectors Of(
        Guid? groupId,
        Guid? chargeStationId,
        IReadOnlyCollection<int>? connectorIds
    )
    {
        groupId.NotBeNull().NotBeEmpty();
        chargeStationId.NotBeNull().NotBeEmpty();
        connectorIds.NotBeNull();

        return new RemoveStationConnectors(groupId.Value, chargeStationId.Value, connectorIds);
    }
}

public sealed class RemoveStationConnectorsHandler(
    IUnitOfWork unitOfWork,
    ILogger<RemoveStationConnectorsHandler> logger
)
{
    public async Task Handle(RemoveStationConnectors removeStationConnectors, CancellationToken cancellationToken)
    {
        removeStationConnectors.NotBeNull();

        // Business rules validation in value objects and entities will do in handlers, not commands, and in command we just have input validations
        var group = await unitOfWork.GroupRepository.GetByIdAsync(
            GroupId.Of(removeStationConnectors.GroupId),
            cancellationToken
        );
        if (group is null)
        {
            throw new NotFoundException($"Group with ID {removeStationConnectors.GroupId} not found.");
        }

        group.RemoveConnectors(
            ChargeStationId.Of(removeStationConnectors.ChargeStationId),
            removeStationConnectors.ConnectorIds.Select(ConnectorId.Of).ToList()
        );

        unitOfWork.GroupRepository.Update(group);
        await unitOfWork.CommitAsync(cancellationToken);

        logger.LogInformation(
            "Connectors successfully removed from ChargeStation {ChargeStationId} in Group {GroupId}. Removed Connectors: {ConnectorIds}",
            removeStationConnectors.ChargeStationId,
            removeStationConnectors.GroupId,
            string.Join(", ", removeStationConnectors.ConnectorIds.Select(id => id))
        );
    }
}
