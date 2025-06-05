using SmartCharging.Groups.Contracts;
using SmartCharging.Groups.Dtos;
using SmartCharging.Groups.Models;
using SmartCharging.Shared.Application.Contratcs;
using SmartCharging.Shared.Application.Data;
using SmartCharging.Shared.BuildingBlocks.Exceptions;
using SmartCharging.Shared.BuildingBlocks.Extensions;

namespace SmartCharging.Groups.Features.CreateGroup.v1;

public record CreateGroup(string Name, int CapacityInAmps, ChargeStationDto? ChargeStation)
{
    public Guid GroupId { get; } = Guid.CreateVersion7();

    public static CreateGroup Of(string? name, int capacityInAmps, ChargeStationDto? chargeStation)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Name cannot be null or empty");

        if (name.Length > 100)
            throw new ValidationException("Name cannot be longer than 100 characters");

        if (capacityInAmps <= 0)
            throw new ValidationException($"Current `{capacityInAmps}` must be greater than 0");

        return new CreateGroup(name, capacityInAmps, chargeStation);
    }
};

public class CreateGroupHandler(
    IUnitOfWork unitOfWork,
    ILogger<CreateGroupHandler> logger,
    IBusinessRuleValidator ruleValidator
)
{
    public async Task<CreateGroupResult> Handle(CreateGroup createGroup, CancellationToken cancellationToken)
    {
        createGroup.NotBeNull();
        var group = new Group
        {
            Id = createGroup.GroupId,
            Name = createGroup.Name,
            CapacityInAmps = createGroup.CapacityInAmps,
        };

        if (createGroup.ChargeStation is not null)
        {
            AddChargeStation(group, createGroup.ChargeStation, ruleValidator);
        }

        await unitOfWork.GroupRepository.AddAsync(group, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        logger.LogInformation(
            "Created group {GroupId} with {StationCount} stations",
            group.Id,
            group.ChargeStations.Count
        );

        return new CreateGroupResult(group.Id);
    }

    private static void AddChargeStation(
        Group group,
        ChargeStationDto chargeStationDto,
        IBusinessRuleValidator ruleValidator
    )
    {
        chargeStationDto.NotBeNull();

        var chargeStation = chargeStationDto.ToChargeStation();

        // Validate charge station uniqueness
        ruleValidator.ValidateChargeStationUniqueness(group, chargeStation.Id);

        // Validate connector configuration
        ruleValidator.ValidateConnectorConfiguration(chargeStation.Connectors);

        // Validate group capacity for the new station
        ruleValidator.ValidateCapacityForAdditions(group, chargeStation.GetTotalCurrent());

        // Add the charge station to the group
        group.ChargeStations.Add(chargeStation);
    }
}

public record CreateGroupResult(Guid GroupId);
