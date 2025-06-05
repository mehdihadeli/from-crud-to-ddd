using SmartCharging.Groups.Contracts;
using SmartCharging.Groups.Models;
using SmartCharging.Shared.Application.Contratcs;
using SmartCharging.Shared.Application.Data;
using SmartCharging.Shared.BuildingBlocks.Exceptions;
using SmartCharging.Shared.BuildingBlocks.Extensions;

namespace SmartCharging.Groups.Features.UpdateGroup.v1;

public record UpdateGroup(Guid GroupId, string Name, int CapacityInAmps)
{
    public static UpdateGroup Of(Guid? groupId, string? name, int capacityInAmps)
    {
        groupId.NotBeNull().NotBeEmpty();
        name.NotBeNullOrWhiteSpace();
        capacityInAmps.NotBeNegativeOrZero();

        return new UpdateGroup(groupId.Value, name, capacityInAmps);
    }
}

public class UpdateGroupHandler(IUnitOfWork unitOfWork, ILogger<UpdateGroupHandler> logger)
{
    public async Task Handle(UpdateGroup updateGroup, CancellationToken cancellationToken)
    {
        updateGroup.NotBeNull();

        var group = await unitOfWork.GroupRepository.GetByIdAsync(updateGroup.GroupId, cancellationToken);

        if (group == null)
            throw new NotFoundException($"Group with ID {updateGroup.GroupId} not found.");

        UpdateGroup(group, updateGroup.Name, updateGroup.CapacityInAmps);

        unitOfWork.GroupRepository.Update(group);
        await unitOfWork.CommitAsync(cancellationToken);

        logger.LogInformation("Group {GroupId} updated.", group.Id);
    }

    private static void UpdateGroup(Group group, string newName, int newCapacity)
    {
        if (newCapacity < group.GetTotalCurrent())
            throw new DomainException("New capacity cannot be less than current usage");

        group.CapacityInAmps = newCapacity;
        group.Name = newName;
    }
}
