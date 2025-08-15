namespace SmartChargingStatisticsApi.Dtos;

record GroupCapacityStatisticsClientResponse(
    Guid GroupId,
    int CurrentLoadAmps,
    int MaxCapacityAmps,
    int AvailableCapacityAmps
);
