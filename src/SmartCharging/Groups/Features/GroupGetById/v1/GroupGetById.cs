using SmartCharging.Groups.Dtos;
using SmartCharging.Groups.Models.ValueObjects;
using SmartCharging.Shared.Application.Data;
using SmartCharging.Shared.BuildingBlocks.Exceptions;
using SmartCharging.Shared.BuildingBlocks.Extensions;

namespace SmartCharging.Groups.Features.GroupGetById.v1;

public sealed record GroupGetById(Guid GroupId)
{
    // - just input validation inside command static constructor and business rules or domain-level validation to the command-handler and construct the Value Objects/Entities within the command-handler
    public static GroupGetById Of(Guid? groupId)
    {
        groupId.NotBeNull().NotBeEmpty();
        return new GroupGetById(groupId.Value);
    }
}

public sealed class GroupGetByIdHandler(IUnitOfWork unitOfWork)
{
    public async Task<GroupGetByIdResult> Handle(GroupGetById groupGetById, CancellationToken cancellationToken)
    {
        groupGetById.NotBeNull();

        // Business rules validation in value objects and entities will do in handlers, not commands, and in command we just have input validations
        var group = await unitOfWork.GroupRepository.GetByIdAsync(GroupId.Of(groupGetById.GroupId), cancellationToken);
        if (group == null)
        {
            throw new NotFoundException($"Group with ID {groupGetById.GroupId} not found.");
        }

        return group.ToGroupGetByIdResult();
    }
}

public sealed record GroupGetByIdResult(GroupDto Group);
