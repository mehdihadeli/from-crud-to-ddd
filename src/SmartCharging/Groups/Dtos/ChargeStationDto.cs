namespace SmartCharging.Groups.Dtos;

public sealed record ChargeStationDto(Guid ChargeStationId, string Name, IReadOnlyCollection<ConnectorDto> Connectors);
