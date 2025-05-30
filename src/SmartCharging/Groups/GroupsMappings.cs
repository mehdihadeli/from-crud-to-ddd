using SmartCharging.Groups.Dtos;
using SmartCharging.Groups.Features.GroupGetById.v1;
using SmartCharging.Groups.Models;
using SmartCharging.Groups.Models.ValueObjects;
using SmartCharging.Shared.BuildingBlocks.Extensions;

namespace SmartCharging.Groups;

public static class GroupsMappings
{
    public static ChargeStation ToChargeStation(this ChargeStationDto chargeStationDto)
    {
        chargeStationDto.Connectors.NotBeNull();

        var connectors = chargeStationDto.Connectors.Select(x =>
            Connector.Create(
                ConnectorId.Of(x.ConnectorId),
                CurrentInAmps.Of(x.MaxCurrentInAmps),
                ChargeStationId.Of(chargeStationDto.ChargeStationId)
            )
        );
        var chargeStation = ChargeStation.Create(
            ChargeStationId.Of(chargeStationDto.ChargeStationId),
            Name.Of(chargeStationDto.Name),
            connectors.ToList().AsReadOnly()
        );

        return chargeStation;
    }

    public static ChargeStationDto ToChargeStationDto(this ChargeStation chargeStation)
    {
        chargeStation.NotBeNull();

        return new ChargeStationDto(
            ChargeStationId: chargeStation.Id.Value,
            Name: chargeStation.Name.Value,
            Connectors: chargeStation.Connectors.Select(c => c.ToConnectorDto()).ToList().AsReadOnly()
        );
    }

    public static Connector ToConnector(this ConnectorDto connectorDto)
    {
        connectorDto.NotBeNull();
        return Connector.Create(
            ConnectorId.Of(connectorDto.ConnectorId),
            CurrentInAmps.Of(connectorDto.MaxCurrentInAmps),
            ChargeStationId.Of(connectorDto.ChargeStationId)
        );
    }

    public static IReadOnlyCollection<Connector> ToConnectors(this IReadOnlyCollection<ConnectorDto>? connectorsDto)
    {
        connectorsDto.NotBeNull();

        return connectorsDto.Select(x => x.ToConnector()).ToList().AsReadOnly();
    }

    public static ConnectorDto ToConnectorDto(this Connector connector)
    {
        connector.NotBeNull();

        ChargeStationId connectorChargeStationId = connector.ChargeStationId;
        return new ConnectorDto(
            ConnectorId: connector.Id.Value,
            MaxCurrentInAmps: connector.MaxCurrentInAmps.Value,
            ChargeStationId: connectorChargeStationId.Value
        );
    }

    public static IReadOnlyCollection<ConnectorDto> ToConnectorsDto(this IReadOnlyCollection<Connector> connectors)
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
            GroupId: group.Id.Value,
            Name: group.Name.Value,
            CapacityInAmps: group.CapacityInAmps.Value,
            ChargeStationsCount: group.ChargeStations.Count,
            ChargeStations: group
                .ChargeStations.Select(cs => new ChargeStationDto(
                    ChargeStationId: cs.Id.Value,
                    Name: cs.Name.Value,
                    Connectors: cs.Connectors.Select(connector => connector.ToConnectorDto()).ToList()
                ))
                .ToList()
                .AsReadOnly()
        );
    }
}
