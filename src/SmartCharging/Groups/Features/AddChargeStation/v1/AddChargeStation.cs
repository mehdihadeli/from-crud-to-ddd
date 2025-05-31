using SmartCharging.Groups.Contracts;
using SmartCharging.Groups.Dtos;
using SmartCharging.Groups.Models;
using SmartCharging.Groups.Models.ValueObjects;
using SmartCharging.Shared.BuildingBlocks.Exceptions;
using SmartCharging.Shared.BuildingBlocks.Extensions;

namespace SmartCharging.Groups.Features.AddChargeStation.v1;

public record AddChargeStation(Guid GroupId, string Name, IReadOnlyCollection<ConnectorDto> Connectors)
{
    // - just input validation inside command static constructor and business rules or domain-level validation to the command-handler and construct the Value Objects/Entities within the command-handler
    // - we can also use FluentValidation for validate basic input validation, not domain validations
    // - `Command` is an application-layer construct designed to transfer and input validation raw user input into the system not to execute domain-level logic which are in ValueObjects/Entities
    // - If we use VO Any change in **Value Objects** (e.g., added properties or altered constructors) may affects to the `Command` behavior.
    // - If we use entities and value objects inside command, we're mixing Domain Validation with Input Validation
    // - If commands are exposed over external boundaries (e.g., messaging queues), embedding **Value Objects** directly into the command can introduce challenges with serialization and deserialization

    public static AddChargeStation Of(Guid? groupId, string? name, IReadOnlyCollection<ConnectorDto>? connectors)
    {
        groupId.NotBeNull().NotBeEmpty();
        name.NotBeEmptyOrNull();
        connectors.NotBeNull();

        return new AddChargeStation(groupId.Value, name, connectors.ToList());
    }

    public Guid ChargeStationId { get; } = Guid.CreateVersion7();
}

public class AddChargeStationHandler(IUnitOfWork unitOfWork, ILogger<AddChargeStationHandler> logger)
{
    public async Task<Guid> Handle(AddChargeStation addChargeStation, CancellationToken cancellationToken)
    {
        addChargeStation.NotBeNull();

        // Business rules validation in value objects and entities will do in handlers, not commands, and in command we just have input validations
        var group = await unitOfWork.GroupRepository.GetByIdAsync(
            GroupId.Of(addChargeStation.GroupId),
            cancellationToken
        );
        if (group is null)
        {
            throw new NotFoundException($"Group with ID {addChargeStation.GroupId} not found.");
        }

        var chargeStation = ChargeStation.Create(
            ChargeStationId.Of(addChargeStation.ChargeStationId),
            Name.Of(addChargeStation.Name),
            addChargeStation.Connectors.ToConnectors()
        );
        group.AddChargeStation(chargeStation);

        // Mark the group as updated
        unitOfWork.GroupRepository.Update(group);
        await unitOfWork.CommitAsync(cancellationToken);

        logger.LogInformation(
            "Charge station {ChargeStationId} with {ConnectorCount} connectors added to group {GroupId}.",
            addChargeStation.ChargeStationId,
            addChargeStation.Connectors.Count,
            addChargeStation.GroupId
        );

        return addChargeStation.ChargeStationId;
    }
}
