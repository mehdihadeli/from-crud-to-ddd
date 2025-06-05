using SmartCharging.Groups.Contracts;
using SmartCharging.Shared.Application.Contratcs;
using SmartCharging.Shared.Application.Data;
using SmartCharging.Shared.BuildingBlocks.Exceptions;
using SmartCharging.Shared.BuildingBlocks.Extensions;

namespace SmartCharging.Groups.Features.RemoveGroup.v1;

public record RemoveGroup(Guid GroupId)
{
    public static RemoveGroup Of(Guid? groupId)
    {
        groupId.NotBeNull().NotBeEmpty();

        return new RemoveGroup(groupId.Value);
    }
}

public class RemoveGroupHandler(IUnitOfWork unitOfWork, ILogger<RemoveGroupHandler> logger)
{
    public async Task Handle(RemoveGroup removeGroup, CancellationToken cancellationToken)
    {
        removeGroup.NotBeNull();

        var group = await unitOfWork.GroupRepository.GetByIdAsync(removeGroup.GroupId, cancellationToken);

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
