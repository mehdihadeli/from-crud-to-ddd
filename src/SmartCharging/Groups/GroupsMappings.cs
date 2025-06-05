using SmartCharging.Groups.Dtos;
using SmartCharging.Groups.Features.GroupGetById.v1;
using SmartCharging.Groups.Models;
using SmartCharging.Shared.BuildingBlocks.Exceptions;
using SmartCharging.Shared.BuildingBlocks.Extensions;

namespace SmartCharging.Groups;

public static class GroupsMappings
{
    public static ChargeStation ToChargeStation(this ChargeStationDto? chargeStationDto)
    {
        chargeStationDto.NotBeNull();

        ValidateChargeStationDto(chargeStationDto);

        var connectors = chargeStationDto.Connectors.Select(c => c.ToConnector()).ToList();
        return new ChargeStation
        {
            Name = chargeStationDto.Name,
            Id = chargeStationDto.ChargeStationId,
            GroupId = chargeStationDto.GroupId,
            Connectors = connectors,
        };
    }

    public static ChargeStationDto ToChargeStationDto(this ChargeStation chargeStation)
    {
        chargeStation.NotBeNull();

        return new ChargeStationDto(
            GroupId: chargeStation.GroupId,
            ChargeStationId: chargeStation.Id,
            Name: chargeStation.Name,
            Connectors: chargeStation.Connectors.Select(c => c.ToConnectorDto()).ToList().AsReadOnly()
        );
    }

    public static Connector ToConnector(this ConnectorDto connectorDto)
    {
        connectorDto.NotBeNull();
        ValidateConnectorDto(connectorDto);

        return new Connector
        {
            MaxCurrentInAmps = connectorDto.MaxCurrentInAmps,
            ChargeStationId = connectorDto.ChargeStationId,
            Id = connectorDto.ConnectorId,
        };
    }

    public static IList<Connector> ToConnectors(this IReadOnlyCollection<ConnectorDto> connectorsDto)
    {
        connectorsDto.NotBeNull();

        return connectorsDto.Select(x => x.ToConnector()).ToList().AsReadOnly();
    }

    public static ConnectorDto ToConnectorDto(this Connector connector)
    {
        connector.NotBeNull();

        return new ConnectorDto(
            ConnectorId: connector.Id,
            MaxCurrentInAmps: connector.MaxCurrentInAmps,
            ChargeStationId: connector.ChargeStationId
        );
    }

    public static IReadOnlyCollection<ConnectorDto> ToConnectorsDto(this IList<Connector> connectors)
    {
        connectors.NotBeNull();

        return connectors.Select(connector => connector.ToConnectorDto()).ToList().AsReadOnly();
    }

    public static GroupGetByIdResult ToGroupGetByIdResult(this Group group)
    {
        group.NotBeNull();

        return new GroupGetByIdResult(group.ToGroupDto());
    }

    public static GroupDto ToGroupDto(this Group group)
    {
        group.NotBeNull();

        return new GroupDto(
            GroupId: group.Id,
            Name: group.Name,
            CapacityInAmps: group.CapacityInAmps,
            ChargeStationsCount: group.ChargeStations.Count,
            ChargeStations: group.ChargeStations.Select(cs => cs.ToChargeStationDto()).ToList().AsReadOnly()
        );
    }

    private static void ValidateChargeStationDto(ChargeStationDto chargeStationDto)
    {
        if (chargeStationDto.ChargeStationId == Guid.Empty)
            throw new ValidationException("Charge Station ID cannot be null or empty");

        if (string.IsNullOrWhiteSpace(chargeStationDto.Name))
            throw new ValidationException("Name cannot be null or empty");

        if (chargeStationDto.Name.Length > 100)
            throw new ValidationException("Name cannot be longer than 100 characters");
    }

    private static void ValidateConnectorDto(ConnectorDto connectorDto)
    {
        if (connectorDto.ConnectorId < 1 || connectorDto.ConnectorId > 5)
            throw new ValidationException(
                $"Connector ID must be between 1 and 5; it is currently '{connectorDto.ConnectorId}'"
            );

        if (connectorDto.MaxCurrentInAmps <= 0)
            throw new ValidationException($"Current `{connectorDto.MaxCurrentInAmps}` must be greater than 0");

        if (connectorDto.ChargeStationId == Guid.Empty)
            throw new ValidationException("Charge Station ID cannot be null or empty");
    }
}
