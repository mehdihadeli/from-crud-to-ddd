namespace SmartCharging.Groups.Dtos;

public record GroupDto(
    Guid GroupId,
    string Name,
    int CapacityInAmps,
    int ChargeStationsCount,
    IReadOnlyCollection<ChargeStationDto> ChargeStations
);
