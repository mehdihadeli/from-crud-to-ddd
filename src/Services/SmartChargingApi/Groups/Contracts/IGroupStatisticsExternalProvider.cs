using SmartChargingApi.Groups.Dtos;

namespace SmartChargingApi.Groups.Contracts;

public interface IGroupStatisticsExternalProvider
{
    Task<GroupCapacityStatisticsDto?> GetCapacityStatisticsAsync(Guid groupId);
    Task<GroupEnergyConsumptionDto?> GetEnergyConsumptionAsync(Guid groupId);
}
