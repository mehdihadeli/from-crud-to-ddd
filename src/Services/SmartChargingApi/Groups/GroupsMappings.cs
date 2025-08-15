using SmartCharging.ServiceDefaults.Extensions;
using SmartChargingApi.Groups.Dtos;
using SmartChargingApi.Groups.Dtos.Response;
using SmartChargingApi.Groups.Features.AddChargeStation.v1;
using SmartChargingApi.Groups.Features.AddStationConnectors.v1;
using SmartChargingApi.Groups.Features.CreateGroup.v1;
using SmartChargingApi.Groups.Features.GetGroupById.v1;
using SmartChargingApi.Groups.Models;
using SmartChargingApi.Groups.Models.ValueObjects;

namespace SmartChargingApi.Groups;

public static class GroupsMappings
{
    public static ChargeStation ToChargeStation(this ChargeStationDto chargeStationDto)
    {
        chargeStationDto.Connectors.NotBeNull();

        var connectors = chargeStationDto.Connectors.Select(x =>
            Connector.Create(ConnectorId.Of(x.ConnectorId), CurrentInAmps.Of(x.MaxCurrentInAmps))
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
            CurrentInAmps.Of(connectorDto.MaxCurrentInAmps)
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

        return new ConnectorDto(ConnectorId: connector.Id.Value, MaxCurrentInAmps: connector.MaxCurrentInAmps.Value);
    }

    public static IReadOnlyCollection<ConnectorDto> ToConnectorsDto(this IReadOnlyCollection<Connector> connectors)
    {
        connectors.NotBeNull();

        return connectors.Select(connector => connector.ToConnectorDto()).ToList().AsReadOnly();
    }

    public static GroupGetByIdResult ToGroupGetByIdResult(
        this Group group,
        GroupCapacityStatisticsDto? capacityStats,
        GroupEnergyConsumptionDto? energyStats
    )
    {
        group.NotBeNull();

        return new GroupGetByIdResult(group.ToGroupDto(), energyStats, capacityStats);
    }

    public static GroupDto ToGroupDto(this Group group)
    {
        group.NotBeNull();

        return new GroupDto(
            GroupId: group.Id.Value,
            Name: group.Name.Value,
            CapacityInAmps: group.CapacityInAmps.Value,
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

    public static ChargeStationDto ToChargeStationDto(
        this CreateGroupRequest.CreateChargeStationRequest createChargeStationRequest
    )
    {
        createChargeStationRequest.NotBeNull();

        return new ChargeStationDto(
            ChargeStationId: createChargeStationRequest.ChargeStationId,
            Name: createChargeStationRequest.Name,
            Connectors: createChargeStationRequest
                .Connectors.Select(c => new ConnectorDto(
                    ConnectorId: c.ConnectorId,
                    MaxCurrentInAmps: c.MaxCurrentInAmps
                ))
                .ToList()
        );
    }

    public static IReadOnlyCollection<ConnectorDto>? ToConnectorsDto(
        this IEnumerable<AddChargeStationRequest.CreateConnectorRequest>? connectorsRequest
    )
    {
        if (connectorsRequest is null)
            return null;

        return connectorsRequest.Select(c => new ConnectorDto(c.ConnectorId, c.MaxCurrentInAmps)).ToList().AsReadOnly();
    }

    public static IReadOnlyCollection<ConnectorDto>? ToConnectorsDto(
        this IEnumerable<AddConnectorRequest.CreateConnectorRequest>? connectorsRequest
    )
    {
        if (connectorsRequest is null)
            return null;

        return connectorsRequest.Select(c => new ConnectorDto(c.ConnectorId, c.MaxCurrentInAmps)).ToList().AsReadOnly();
    }

    public static ConnectorResponseDto ToConnectorResponse(this ConnectorDto connector)
    {
        return new ConnectorResponseDto(connector.ConnectorId, connector.MaxCurrentInAmps);
    }

    public static ChargeStationResponseDto ToChargeStationResponse(this ChargeStationDto chargeStation)
    {
        return new ChargeStationResponseDto(
            chargeStation.ChargeStationId,
            chargeStation.Name,
            [.. chargeStation.Connectors.Select(x => x.ToConnectorResponse())]
        );
    }

    public static GroupEnergyConsumptionResponseDto ToGroupEnergyConsumptionResponse(
        this GroupEnergyConsumptionDto energyConsumption
    )
    {
        return new GroupEnergyConsumptionResponseDto(
            energyConsumption.EnergyUsedKWh,
            energyConsumption.PeriodStart,
            energyConsumption.PeriodEnd
        );
    }

    public static GroupCapacityStatisticsResponseDto ToGroupCapacityStatisticsResponse(
        this GroupCapacityStatisticsDto capacityStats
    )
    {
        return new GroupCapacityStatisticsResponseDto(
            capacityStats.CurrentLoadAmps,
            capacityStats.MaxCapacityAmps,
            capacityStats.AvailableCapacityAmps
        );
    }
}
