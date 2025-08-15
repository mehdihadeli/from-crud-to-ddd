namespace SmartChargingApi.Groups.Dtos.Response;

public record GroupEnergyConsumptionResponseDto(double EnergyUsedKWh, DateTime PeriodStart, DateTime PeriodEnd);
