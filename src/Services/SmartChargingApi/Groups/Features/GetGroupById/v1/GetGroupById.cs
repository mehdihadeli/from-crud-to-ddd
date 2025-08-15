using SmartCharging.ServiceDefaults.Exceptions;
using SmartCharging.ServiceDefaults.Extensions;
using SmartChargingApi.Groups.Contracts;
using SmartChargingApi.Groups.Dtos;
using SmartChargingApi.Groups.Models.ValueObjects;
using SmartChargingApi.Shared.Contracts;

namespace SmartChargingApi.Groups.Features.GetGroupById.v1;

public sealed record GetGroupById(Guid GroupId)
{
    // - just input validation inside command static constructor and business rules or domain-level validation to the command-handler and construct the Value Objects/Entities within the command-handler
    public static GetGroupById Of(Guid? groupId)
    {
        groupId.NotBeNull().NotBeEmpty();
        return new GetGroupById(groupId.Value);
    }
}

public sealed class GetGroupByIdHandler(
    IUnitOfWork unitOfWork,
    IGroupStatisticsExternalProvider statsProvider,
    ILogger<GetGroupByIdHandler> logger
)
{
    public async Task<GroupGetByIdResult> Handle(GetGroupById getGroupById, CancellationToken cancellationToken)
    {
        getGroupById.NotBeNull();

        // Business rules validation in value objects and entities will do in handlers, not commands, and in command we just have input validations
        var group = await unitOfWork.GroupRepository.GetByIdAsync(GroupId.Of(getGroupById.GroupId), cancellationToken);
        if (group == null)
        {
            throw new NotFoundException($"Group with ID {getGroupById.GroupId} not found.");
        }

        var capacityStats = await statsProvider.GetCapacityStatisticsAsync(group.Id.Value);
        var energyStats = await statsProvider.GetEnergyConsumptionAsync(group.Id.Value);

        return group.ToGroupGetByIdResult(capacityStats, energyStats);
    }
}

public sealed record GroupGetByIdResult(
    GroupDto Group,
    GroupEnergyConsumptionDto? EnergyStats,
    GroupCapacityStatisticsDto? CapacityStats
);
