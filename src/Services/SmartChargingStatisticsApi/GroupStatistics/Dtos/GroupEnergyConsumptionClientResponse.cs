namespace SmartChargingStatisticsApi.GroupStatistics.Dtos;

record GroupEnergyConsumptionClientResponse(
    Guid GroupId,
    double EnergyUsedKWh,
    DateTime PeriodStart,
    DateTime PeriodEnd
);
