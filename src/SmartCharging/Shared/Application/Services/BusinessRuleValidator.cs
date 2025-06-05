using SmartCharging.Groups.Dtos;
using SmartCharging.Groups.Models;
using SmartCharging.Shared.Application.Contratcs;
using SmartCharging.Shared.BuildingBlocks.Exceptions;

namespace SmartCharging.Shared.Application.Services;

public class BusinessRuleValidator : IBusinessRuleValidator
{
    public void ValidateGroupCapacity(int capacityInAmps, int currentLoad, int additionalLoad)
    {
        if (currentLoad + additionalLoad > capacityInAmps)
        {
            throw new DomainException("Adding this station/connector would exceed group capacity.");
        }
    }

    public void ValidateChargeStationUniqueness(Group group, Guid chargeStationId)
    {
        if (group.ChargeStations.Any(s => s.Id == chargeStationId))
        {
            throw new DomainException("Charge station with this ID already exists in the group.");
        }
    }

    public void ValidateConnectorConfiguration(IList<Connector> connectors)
    {
        if (connectors.Count < 1)
        {
            throw new DomainException("Charge station must have at least one connector.");
        }

        foreach (var connector in connectors)
        {
            if (connector.MaxCurrentInAmps <= 0)
            {
                throw new DomainException($"Connector current `{connector.MaxCurrentInAmps}` must be greater than 0.");
            }

            if (connector.Id < 1 || connector.Id > 5)
            {
                throw new ValidationException($"Connector ID must be between 1 and 5, but is {connector.Id}.");
            }
        }
    }

    public void ValidateConnectorUniqueness(
        IList<Connector> existingConnectors,
        IReadOnlyCollection<ConnectorDto> newConnectors
    )
    {
        var existingIds = existingConnectors.Select(c => c.Id).ToHashSet();
        var newIds = newConnectors.Select(c => c.ConnectorId).ToHashSet();

        if (existingIds.Intersect(newIds).Any())
        {
            throw new DomainException("Connector IDs must be unique within a charge station.");
        }
    }

    public void ValidateCapacityForAdditions(Group group, int additionalLoad)
    {
        int currentTotalCurrent = group.GetTotalCurrent();
        ValidateGroupCapacity(group.CapacityInAmps, currentTotalCurrent, additionalLoad);
    }
}
