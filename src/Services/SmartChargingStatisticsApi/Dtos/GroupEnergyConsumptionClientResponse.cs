namespace SmartChargingStatisticsApi.Dtos;

record GroupEnergyConsumptionClientResponse(
    Guid GroupId,
    double EnergyUsedKWh,
    DateTime PeriodStart,
    DateTime PeriodEnd
);
