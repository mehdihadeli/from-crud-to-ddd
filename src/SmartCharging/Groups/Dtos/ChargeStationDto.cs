namespace SmartCharging.Groups.Dtos;

public record ChargeStationDto(
    Guid GroupId,
    Guid ChargeStationId,
    string Name,
    IReadOnlyCollection<ConnectorDto> Connectors
);
