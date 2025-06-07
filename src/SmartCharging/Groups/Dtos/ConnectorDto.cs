namespace SmartCharging.Groups.Dtos;

public sealed record ConnectorDto(Guid ChargeStationId, int ConnectorId, int MaxCurrentInAmps);
