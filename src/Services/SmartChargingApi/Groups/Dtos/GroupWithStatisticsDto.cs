namespace SmartChargingApi.Groups.Dtos;

public record GroupWithStatisticsDto(
    Guid GroupId,
    string Name,
    int Capacity,
    GroupCapacityStatisticsDto? CapacityStatistics,
    GroupEnergyConsumptionDto? EnergyConsumption
);
