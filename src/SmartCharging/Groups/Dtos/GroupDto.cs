namespace SmartCharging.Groups.Dtos;

public sealed record GroupDto(
    Guid GroupId,
    string Name,
    int CapacityInAmps,
    IReadOnlyCollection<ChargeStationDto> ChargeStations
);
