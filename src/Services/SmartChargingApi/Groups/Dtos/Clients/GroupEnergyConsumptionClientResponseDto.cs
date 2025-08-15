namespace SmartChargingApi.Groups.Dtos.Clients;

public record GroupEnergyConsumptionClientResponseDto(
    Guid GroupId,
    double EnergyUsedKWh,
    DateTime PeriodStart,
    DateTime PeriodEnd
);
