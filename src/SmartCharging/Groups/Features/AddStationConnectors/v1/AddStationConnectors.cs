using SmartCharging.Groups.Contracts;
using SmartCharging.Groups.Dtos;
using SmartCharging.Groups.Models;
using SmartCharging.Groups.Models.ValueObjects;
using SmartCharging.Shared.BuildingBlocks.Exceptions;
using SmartCharging.Shared.BuildingBlocks.Extensions;

namespace SmartCharging.Groups.Features.AddStationConnectors.v1;

public record AddStationConnectors(
    GroupId GroupId,
    ChargeStationId ChargeStationId,
    IReadOnlyCollection<Connector> Connectors
)
{
    public static AddStationConnectors Of(
        Guid? groupId,
        Guid? chargeStationId,
        IReadOnlyCollection<ConnectorDto>? connectors
    )
    {
        groupId.NotBeNull();
        chargeStationId.NotBeNull();
        connectors.NotBeNull();

        return new AddStationConnectors(
            GroupId.Of(groupId.Value),
            ChargeStationId.Of(chargeStationId.Value),
            connectors.ToConnectors()
        );
    }
}

public class AddConnectorsHandler(IUnitOfWork unitOfWork, ILogger<AddConnectorsHandler> logger)
{
    public async Task<AddStationConnectorsResult> Handle(
        AddStationConnectors addStationConnectors,
        CancellationToken cancellationToken
    )
    {
        addStationConnectors.NotBeNull();

        var group = await unitOfWork.GroupRepository.GetByIdAsync(addStationConnectors.GroupId, cancellationToken);
        if (group is null)
            throw new NotFoundException($"Group with ID {addStationConnectors.GroupId.Value} not found.");

        group.AddConnectors(addStationConnectors.ChargeStationId, addStationConnectors.Connectors.ToList());

        unitOfWork.GroupRepository.Update(group);
        await unitOfWork.CommitAsync(cancellationToken);

        logger.LogInformation(
            "Connectors successfully added to ChargeStation {ChargeStationId} in Group {GroupId}. Added Connectors: {Connectors}",
            addStationConnectors.ChargeStationId,
            addStationConnectors.GroupId,
            string.Join(", ", addStationConnectors.Connectors.Select(c => c.Id.Value))
        );

        return new AddStationConnectorsResult(addStationConnectors.Connectors.ToConnectorsDto());
    }
}

public record AddStationConnectorsResult(IReadOnlyCollection<ConnectorDto> Connectors);
