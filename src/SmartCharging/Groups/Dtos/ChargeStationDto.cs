namespace SmartCharging.Groups.Dtos;

public record ChargeStationDto(Guid ChargeStationId, string Name, IReadOnlyCollection<ConnectorDto> Connectors);
