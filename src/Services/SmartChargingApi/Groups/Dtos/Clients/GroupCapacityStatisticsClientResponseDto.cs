namespace SmartChargingApi.Groups.Dtos.Clients;

public record GroupCapacityStatisticsClientResponseDto(
    Guid GroupId,
    int CurrentLoadAmps,
    int MaxCapacityAmps,
    int AvailableCapacityAmps
);
