namespace SmartChargingApi.Groups.Dtos.Response;

public sealed record ChargeStationResponseDto(
    Guid ChargeStationId,
    string Name,
    IReadOnlyCollection<ConnectorResponseDto> Connectors
);
