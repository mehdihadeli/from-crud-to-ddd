using SmartCharging.Groups.Dtos;
using SmartCharging.Groups.Models;

namespace SmartCharging.Shared.Application.Contratcs;

public interface IBusinessRuleValidator
{
    void ValidateGroupCapacity(int capacityInAmps, int currentLoad, int additionalLoad);
    void ValidateChargeStationUniqueness(Group group, Guid chargeStationId);
    void ValidateConnectorConfiguration(IList<Connector> connectors);
    void ValidateConnectorUniqueness(
        IList<Connector> existingConnectors,
        IReadOnlyCollection<ConnectorDto> newConnectors
    );

    void ValidateCapacityForAdditions(Group group, int additionalLoad);
}
