using SmartCharging.Groups.Contracts;
using SmartCharging.Groups.Models.ValueObjects;
using SmartCharging.Shared.BuildingBlocks.Exceptions;
using SmartCharging.Shared.BuildingBlocks.Extensions;

namespace SmartCharging.Groups.Features.UpdateGroup.v1;

public record UpdateGroup(GroupId GroupId, Name Name, CurrentInAmps CapacityInAmps)
{
    public static UpdateGroup Of(Guid? groupId, string? name, int capacityInAmps)
    {
        return new UpdateGroup(GroupId.Of(groupId), Name.Of(name), CurrentInAmps.Of(capacityInAmps));
    }
}

public class UpdateGroupHandler(IUnitOfWork unitOfWork, ILogger<UpdateGroupHandler> logger)
{
    public async Task Handle(UpdateGroup updateGroup, CancellationToken cancellationToken)
    {
        updateGroup.NotBeNull();

        var group = await unitOfWork.GroupRepository.GetByIdAsync(updateGroup.GroupId, cancellationToken);

        if (group == null)
            throw new NotFoundException($"Group with ID {updateGroup.GroupId.Value} not found.");

        group.UpdateGroup(updateGroup.Name, updateGroup.CapacityInAmps);

        unitOfWork.GroupRepository.Update(group);
        await unitOfWork.CommitAsync(cancellationToken);

        logger.LogInformation("Group {GroupId} updated.", group.Id.Value);
    }
}
