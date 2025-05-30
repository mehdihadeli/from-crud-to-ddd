namespace SmartCharging.Groups.Dtos;

public record ConnectorDto(Guid ChargeStationId, int ConnectorId, int MaxCurrentInAmps);
