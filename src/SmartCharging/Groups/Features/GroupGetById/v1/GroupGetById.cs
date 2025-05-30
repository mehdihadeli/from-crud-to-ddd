using SmartCharging.Groups.Contracts;
using SmartCharging.Groups.Dtos;
using SmartCharging.Groups.Models.ValueObjects;
using SmartCharging.Shared.BuildingBlocks.Exceptions;
using SmartCharging.Shared.BuildingBlocks.Extensions;

namespace SmartCharging.Groups.Features.GroupGetById.v1;

public record GroupGetById(GroupId GroupId)
{
    public static GroupGetById Of(Guid? groupId)
    {
        return new GroupGetById(GroupId.Of(groupId));
    }
}

public class GroupGetByIdHandler(IUnitOfWork unitOfWork)
{
    public async Task<GroupGetByIdResult> Handle(GroupGetById groupGetById, CancellationToken cancellationToken)
    {
        groupGetById.NotBeNull();

        var group = await unitOfWork.GroupRepository.GetByIdAsync(groupGetById.GroupId, cancellationToken);
        if (group == null)
        {
            throw new NotFoundException($"Group with ID {groupGetById.GroupId.Value} not found.");
        }

        return group.ToGroupGetByIdResult();
    }
}

public record GroupGetByIdResult(GroupDto Group);
