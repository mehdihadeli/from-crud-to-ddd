using SmartCharging.ServiceDefaults.Exceptions;
using SmartCharging.ServiceDefaults.Extensions;
using SmartChargingApi.Groups.Dtos;
using SmartChargingApi.Groups.Models.ValueObjects;
using SmartChargingApi.Shared.Contracts;

namespace SmartChargingApi.Groups.Features.AddStationConnectors.v1;

public sealed record AddStationConnectors(
    Guid GroupId,
    Guid ChargeStationId,
    IReadOnlyCollection<ConnectorDto> Connectors
)
{
    // - just input validation inside command static constructor and business rules or domain-level validation to the command-handler and construct the Value Objects/Entities within the command-handler
    // - we can also use FluentValidation for validate basic input validation, not domain validations
    // - `Command` is an application-layer construct designed to transfer and input validation raw user input into the system not to execute domain-level logic which are in ValueObjects/Entities
    // - If we use VO Any change in **Value Objects** (e.g., added properties or altered constructors) may affects to the `Command` behavior.
    // - If we use entities and value objects inside command, we're mixing Domain Validation with Input Validation
    // - If commands are exposed over external boundaries (e.g., messaging queues), embedding **Value Objects** directly into the command can introduce challenges with serialization and deserialization

    public static AddStationConnectors Of(
        Guid? groupId,
        Guid? chargeStationId,
        IReadOnlyCollection<ConnectorDto>? connectors
    )
    {
        groupId.NotBeNull().NotBeEmpty();
        chargeStationId.NotBeNull().NotBeEmpty();
        connectors.NotBeNull();

        return new AddStationConnectors(groupId.Value, chargeStationId.Value, connectors);
    }
}

public sealed class AddConnectorsHandler(IUnitOfWork unitOfWork, ILogger<AddConnectorsHandler> logger)
{
    public async Task<AddStationConnectorsResult> Handle(
        AddStationConnectors addStationConnectors,
        CancellationToken cancellationToken
    )
    {
        addStationConnectors.NotBeNull();

        // Business rules validation in value objects and entities will do in handlers, not commands, and in command we just have input validations
        var group = await unitOfWork.GroupRepository.GetByIdAsync(
            GroupId.Of(addStationConnectors.GroupId),
            cancellationToken
        );
        if (group is null)
            throw new NotFoundException($"Group with ID {addStationConnectors.GroupId} not found.");

        group.AddConnectors(
            ChargeStationId.Of(addStationConnectors.ChargeStationId),
            addStationConnectors.Connectors.ToConnectors()
        );

        unitOfWork.GroupRepository.Update(group);
        await unitOfWork.CommitAsync(cancellationToken);

        logger.LogInformation(
            "Connectors successfully added to ChargeStation {ChargeStationId} in Group {GroupId}. Added Connectors: {Connectors}",
            addStationConnectors.ChargeStationId,
            addStationConnectors.GroupId,
            string.Join(", ", addStationConnectors.Connectors.Select(c => c.ConnectorId))
        );

        return new AddStationConnectorsResult(addStationConnectors.Connectors);
    }
}

public sealed record AddStationConnectorsResult(IReadOnlyCollection<ConnectorDto> Connectors);
