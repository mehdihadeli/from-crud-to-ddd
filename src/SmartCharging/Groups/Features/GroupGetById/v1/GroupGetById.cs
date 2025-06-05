using SmartCharging.Groups.Contracts;
using SmartCharging.Groups.Dtos;
using SmartCharging.Shared.Application.Contratcs;
using SmartCharging.Shared.Application.Data;
using SmartCharging.Shared.BuildingBlocks.Exceptions;
using SmartCharging.Shared.BuildingBlocks.Extensions;

namespace SmartCharging.Groups.Features.GroupGetById.v1;

public record GroupGetById(Guid GroupId)
{
    public static GroupGetById Of(Guid? groupId)
    {
        groupId.NotBeNull().NotBeEmpty();

        return new GroupGetById(groupId.Value);
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
            throw new NotFoundException($"Group with ID {groupGetById.GroupId} not found.");
        }

        return group.ToGroupGetByIdResult();
    }
}

public record GroupGetByIdResult(GroupDto Group);
