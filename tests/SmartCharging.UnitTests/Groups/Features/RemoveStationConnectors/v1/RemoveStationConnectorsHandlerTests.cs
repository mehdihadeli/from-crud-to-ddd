using Microsoft.Extensions.Logging;
using NSubstitute;
using SmartCharging.Groups.Contracts;
using SmartCharging.Groups.Features.RemoveStationConnectors.v1;
using SmartCharging.Groups.Models;
using SmartCharging.Groups.Models.ValueObjects;
using SmartCharging.Shared.Application.Data;
using SmartCharging.Shared.BuildingBlocks.Exceptions;
using SmartCharging.UnitTests.Groups.Mocks;

namespace SmartCharging.UnitTests.Groups.Features.RemoveStationConnectors.v1;

public class RemoveStationConnectorsHandlerTests
{
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly ILogger<RemoveStationConnectorsHandler> _loggerMock;
    private readonly RemoveStationConnectorsHandler _handler;

    public RemoveStationConnectorsHandlerTests()
    {
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _loggerMock = Substitute.For<ILogger<RemoveStationConnectorsHandler>>();
        _handler = new RemoveStationConnectorsHandler(_unitOfWorkMock, _loggerMock);
    }

    [Fact]
    public async Task Handle_WithValidData_RemovesStationConnectorsSuccessfullyAndCommits()
    {
        // Arrange
        var groupId = GroupId.New();
        var groupName = Name.Of("Test Group");
        var capacityInAmps = CurrentInAmps.Of(500);

        var chargeStation = new ChargeStationFake(3).Generate();
        var group = Group.Create(groupId, groupName, capacityInAmps, chargeStation);

        _unitOfWorkMock.GroupRepository.GetByIdAsync(Arg.Is(groupId), Arg.Any<CancellationToken>()).Returns(group);

        // Get the IDs of the first 2 connectors to remove
        var connectorIdsToRemove = chargeStation.Connectors.Select(c => c.Id).Take(2).ToList();

        var removeStationConnectors =
            new SmartCharging.Groups.Features.RemoveStationConnectors.v1.RemoveStationConnectors(
                groupId.Value,
                chargeStation.Id.Value,
                connectorIdsToRemove.Select(x => x.Value).ToList()
            );

        // Act
        await _handler.Handle(removeStationConnectors, CancellationToken.None);

        // Assert
        // Verify 2 connectors were removed, leaving only 1
        chargeStation.Connectors.Count.ShouldBe(1);

        // Verify repository update and commit
        _unitOfWorkMock.GroupRepository.Received(1).Update(Arg.Is<Group>(g => g.Id == groupId));
        await _unitOfWorkMock.Received(1).CommitAsync(CancellationToken.None);

        // Verify logging
        _loggerMock.Received(1);
    }

    [Fact]
    public async Task Handle_WithNonExistentGroup_ThrowsNotFoundException()
    {
        // Arrange
        var groupId = GroupId.New();
        var chargeStationId = ChargeStationId.New();
        var connectorIds = new List<ConnectorId> { ConnectorId.Of(1) };

        _unitOfWorkMock
            .GroupRepository.GetByIdAsync(Arg.Is(groupId), Arg.Any<CancellationToken>())
            .Returns((Group)null!);

        var removeStationConnectors =
            new SmartCharging.Groups.Features.RemoveStationConnectors.v1.RemoveStationConnectors(
                groupId.Value,
                chargeStationId.Value,
                connectorIds.Select(x => x.Value).ToList()
            );

        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(() =>
            _handler.Handle(removeStationConnectors, CancellationToken.None)
        );

        exception.Message.ShouldBe($"Group with ID {groupId.Value} not found.");

        // Verify no repository update or commit
        _unitOfWorkMock.GroupRepository.DidNotReceiveWithAnyArgs().Update(default!);
        await _unitOfWorkMock.Received(0).CommitAsync(CancellationToken.None);

        // Verify no logging occurred
        _loggerMock.DidNotReceiveWithAnyArgs().LogInformation(default);
    }

    [Fact]
    public async Task Handle_WithNonExistentChargeStation_ThrowsDomainException()
    {
        // Arrange
        var groupId = GroupId.New();
        var chargeStationId = ChargeStationId.New();

        var groupName = Name.Of("Test Group");
        var capacityInAmps = CurrentInAmps.Of(300);

        var group = Group.Create(groupId, groupName, capacityInAmps, chargeStation: null);

        _unitOfWorkMock.GroupRepository.GetByIdAsync(Arg.Is(groupId), Arg.Any<CancellationToken>()).Returns(group);

        var connectorIds = new List<ConnectorId> { ConnectorId.Of(1) };
        var removeStationConnectors =
            new SmartCharging.Groups.Features.RemoveStationConnectors.v1.RemoveStationConnectors(
                groupId.Value,
                chargeStationId.Value,
                connectorIds.Select(x => x.Value).ToList()
            );

        // Act & Assert
        var exception = await Should.ThrowAsync<DomainException>(() =>
            _handler.Handle(removeStationConnectors, CancellationToken.None)
        );

        exception.Message.ShouldBe("Charge station not found");

        // Verify no repository update or commit
        _unitOfWorkMock.GroupRepository.DidNotReceiveWithAnyArgs().Update(default!);
        await _unitOfWorkMock.Received(0).CommitAsync(CancellationToken.None);

        // Verify no logging occurred
        _loggerMock.DidNotReceiveWithAnyArgs().LogInformation(default);
    }

    [Fact]
    public async Task Handle_WithNullInput_ThrowsValidationException()
    {
        // Act & Assert
        await Should.ThrowAsync<ValidationException>(() => _handler.Handle(null!, CancellationToken.None));
    }
}
