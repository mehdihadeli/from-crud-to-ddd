namespace SmartChargingApi.Groups.Dtos;

public record GroupCapacityStatisticsDto(int CurrentLoadAmps, int MaxCapacityAmps, int AvailableCapacityAmps);
