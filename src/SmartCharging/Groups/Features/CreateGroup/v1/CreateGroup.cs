using SmartCharging.Groups.Dtos;
using SmartCharging.Groups.Models;
using SmartCharging.Groups.Models.ValueObjects;
using SmartCharging.Shared.Application.Data;
using SmartCharging.Shared.BuildingBlocks.Extensions;

namespace SmartCharging.Groups.Features.CreateGroup.v1;

public record CreateGroup(string Name, int CapacityInAmps, ChargeStationDto? ChargeStation)
{
    // - just input validation inside command static constructor and business rules or domain-level validation to the command-handler and construct the Value Objects/Entities within the command-handler
    // - we can also use FluentValidation for validate basic input validation, not domain validations
    // - `Command` is an application-layer construct designed to transfer and input validation raw user input into the system not to execute domain-level logic which are in ValueObjects/Entities
    // - If we use VO Any change in **Value Objects** (e.g., added properties or altered constructors) may affects to the `Command` behavior.
    // - If we use entities and value objects inside command, we're mixing Domain Validation with Input Validation
    // - If commands are exposed over external boundaries (e.g., messaging queues), embedding **Value Objects** directly into the command can introduce challenges with serialization and deserialization
    public static CreateGroup Of(string? name, int capacityInAmps, ChargeStationDto? chargeStation)
    {
        name.NotBeEmptyOrNull();
        capacityInAmps.NotBeNegativeOrZero();

        return new CreateGroup(name, capacityInAmps, chargeStation);
    }

    public Guid GroupId { get; } = Guid.CreateVersion7();
}

public class CreateGroupHandler(IUnitOfWork unitOfWork, ILogger<CreateGroupHandler> logger)
{
    public async Task<CreateGroupResult> Handle(CreateGroup createGroup, CancellationToken cancellationToken)
    {
        createGroup.NotBeNull();

        // Business rules validation in value objects and entities will do in handlers, not commands, and in command we just have input validations
        var group = Group.Create(
            GroupId.Of(createGroup.GroupId),
            Name.Of(createGroup.Name),
            CurrentInAmps.Of(createGroup.CapacityInAmps),
            createGroup.ChargeStation?.ToChargeStation()
        );

        await unitOfWork.GroupRepository.AddAsync(group, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        logger.LogInformation(
            "Created group {GroupId} with {StationCount} stations",
            group.Id.Value,
            group.ChargeStations.Count
        );

        return new CreateGroupResult(group.Id.Value);
    }
}

public record CreateGroupResult(Guid GroupId);
