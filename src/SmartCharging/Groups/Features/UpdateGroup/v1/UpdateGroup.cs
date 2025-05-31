using SmartCharging.Groups.Contracts;
using SmartCharging.Groups.Models.ValueObjects;
using SmartCharging.Shared.BuildingBlocks.Exceptions;
using SmartCharging.Shared.BuildingBlocks.Extensions;

namespace SmartCharging.Groups.Features.UpdateGroup.v1;

public record UpdateGroup(Guid GroupId, string Name, int CapacityInAmps)
{
    public static UpdateGroup Of(Guid? groupId, string? name, int capacityInAmps)
    {
        groupId.NotBeNull().NotBeEmpty();
        name.NotBeEmptyOrNull();
        capacityInAmps.NotBeNegativeOrZero();

        return new UpdateGroup(groupId.Value, name, capacityInAmps);
    }
}

public class UpdateGroupHandler(IUnitOfWork unitOfWork, ILogger<UpdateGroupHandler> logger)
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
