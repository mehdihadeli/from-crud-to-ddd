using SmartCharging.Groups.Contracts;
using SmartCharging.Groups.Dtos;
using SmartCharging.Groups.Models;
using SmartCharging.Groups.Models.ValueObjects;
using SmartCharging.Shared.BuildingBlocks.Exceptions;
using SmartCharging.Shared.BuildingBlocks.Extensions;

namespace SmartCharging.Groups.Features.CreateGroup.v1;

public record CreateGroup(Name Name, CurrentInAmps CapacityInAmps, ChargeStation? ChargeStation)
{
    public GroupId GroupId { get; } = GroupId.New();

    public static CreateGroup Of(string? name, int capacityInAmps, ChargeStationDto? chargeStation)
    {
        return new CreateGroup(Name.Of(name), CurrentInAmps.Of(capacityInAmps), chargeStation?.ToChargeStation());
    }
};

public class CreateGroupHandler(IUnitOfWork unitOfWork, ILogger<CreateGroupHandler> logger)
{
    public async Task<CreateGroupResult> Handle(CreateGroup createGroup, CancellationToken cancellationToken)
    {
        createGroup.NotBeNull();

        var group = Group.Create(
            createGroup.GroupId,
            createGroup.Name,
            createGroup.CapacityInAmps,
            createGroup.ChargeStation
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
