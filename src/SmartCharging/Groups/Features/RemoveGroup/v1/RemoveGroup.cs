using SmartCharging.Groups.Models.ValueObjects;
using SmartCharging.Shared.Application.Contracts;
using SmartCharging.Shared.BuildingBlocks.Exceptions;
using SmartCharging.Shared.BuildingBlocks.Extensions;

namespace SmartCharging.Groups.Features.RemoveGroup.v1;

public sealed record RemoveGroup(Guid GroupId)
{
    // - just input validation inside command static constructor and business rules or domain-level validation to the command-handler and construct the Value Objects/Entities within the command-handler
    public static RemoveGroup Of(Guid? groupId)
    {
        groupId.NotBeNull().NotBeEmpty();
        return new RemoveGroup(groupId.Value);
    }
}

public sealed class RemoveGroupHandler(IUnitOfWork unitOfWork, ILogger<RemoveGroupHandler> logger)
{
    public async Task Handle(RemoveGroup removeGroup, CancellationToken cancellationToken)
    {
        removeGroup.NotBeNull();

        // Business rules validation in value objects and entities will do in handlers, not commands, and in command we just have input validations
        var group = await unitOfWork.GroupRepository.GetByIdAsync(GroupId.Of(removeGroup.GroupId), cancellationToken);
        if (group is null)
        {
            throw new NotFoundException($"Group with ID {removeGroup.GroupId} not found.");
        }

        // EF Core cascade-delete will remove associated charge stations and connectors
        unitOfWork.GroupRepository.Remove(group);
        await unitOfWork.CommitAsync(cancellationToken);

        logger.LogInformation("Group {GroupId} and its associated entities were removed.", removeGroup.GroupId);
    }
}
