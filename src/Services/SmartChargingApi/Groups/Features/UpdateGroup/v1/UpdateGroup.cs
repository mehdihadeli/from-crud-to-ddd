using SmartCharging.ServiceDefaults.Exceptions;
using SmartCharging.ServiceDefaults.Extensions;
using SmartChargingApi.Groups.Models.ValueObjects;
using SmartChargingApi.Shared.Contracts;

namespace SmartChargingApi.Groups.Features.UpdateGroup.v1;

public sealed record UpdateGroup(Guid GroupId, string Name, int CapacityInAmps)
{
    public static UpdateGroup Of(Guid? groupId, string? name, int? capacityInAmps)
    {
        groupId.NotBeNull().NotBeEmpty();
        name.NotBeEmptyOrNull();
        capacityInAmps.NotBeNull();
        capacityInAmps.NotBeNegativeOrZero();

        return new UpdateGroup(groupId.Value, name, capacityInAmps.Value);
    }
}

public sealed class UpdateGroupHandler(IUnitOfWork unitOfWork, ILogger<UpdateGroupHandler> logger)
{
    public async Task Handle(UpdateGroup updateGroup, CancellationToken cancellationToken)
    {
        updateGroup.NotBeNull();

        // Business rules validation in value objects and entities will do in handlers, not commands, and in command we just have input validations
        var group = await unitOfWork.GroupRepository.GetByIdAsync(GroupId.Of(updateGroup.GroupId), cancellationToken);

        if (group == null)
            throw new NotFoundException($"Group with ID {updateGroup.GroupId} not found.");

        group.UpdateGroup(Name.Of(updateGroup.Name), CurrentInAmps.Of(updateGroup.CapacityInAmps));

        unitOfWork.GroupRepository.Update(group);
        await unitOfWork.CommitAsync(cancellationToken);

        logger.LogInformation("Group {GroupId} updated.", group.Id.Value);
    }
}
