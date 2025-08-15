namespace SmartChargingApi.Groups.Dtos.Response;

public record GroupCapacityStatisticsResponseDto(int CurrentLoadAmps, int MaxCapacityAmps, int AvailableCapacityAmps);
