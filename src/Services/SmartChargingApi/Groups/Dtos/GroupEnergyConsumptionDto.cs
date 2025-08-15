namespace SmartChargingApi.Groups.Dtos;

public record GroupEnergyConsumptionDto(double EnergyUsedKWh, DateTime PeriodStart, DateTime PeriodEnd);
