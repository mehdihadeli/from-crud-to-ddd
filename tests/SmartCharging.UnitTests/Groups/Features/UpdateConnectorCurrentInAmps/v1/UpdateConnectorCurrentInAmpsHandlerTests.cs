using Microsoft.Extensions.Logging;
using NSubstitute;
using SmartCharging.ServiceDefaults.Exceptions;
using SmartCharging.UnitTests.Groups.Mocks;
using SmartChargingApi.Groups.Features.UpdateConnectorCurrentInAmps.v1;
using SmartChargingApi.Groups.Models;
using SmartChargingApi.Groups.Models.ValueObjects;
using SmartChargingApi.Shared.Contracts;

namespace SmartCharging.UnitTests.Groups.Features.UpdateConnectorCurrentInAmps.v1;

public class UpdateConnectorCurrentInAmpsHandlerTests
{
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly ILogger<UpdateConnectorCurrentInAmpsHandler> _loggerMock;
    private readonly UpdateConnectorCurrentInAmpsHandler _handler;

    public UpdateConnectorCurrentInAmpsHandlerTests()
    {
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _loggerMock = Substitute.For<ILogger<UpdateConnectorCurrentInAmpsHandler>>();
        _handler = new UpdateConnectorCurrentInAmpsHandler(_unitOfWorkMock, _loggerMock);
    }

    [Fact]
    public async Task Handle_WithValidData_UpdatesConnectorCurrentSuccessfullyAndCommits()
    {
        // Arrange
        var groupId = GroupId.New();
        var chargeStationId = ChargeStationId.New();
        var connectorId = ConnectorId.Of(1);
        var newCurrent = CurrentInAmps.Of(25);

        var groupName = Name.Of("Test Group");
        var capacityInAmps = CurrentInAmps.Of(500);

        var chargeStation = new ChargeStationFake(2).Generate();
        chargeStation.Id = chargeStationId;
        var connectorToUpdate = chargeStation.Connectors.First(c => c.Id == connectorId);

        var group = Group.Create(groupId, groupName, capacityInAmps, chargeStation);

        _unitOfWorkMock.GroupRepository.GetByIdAsync(Arg.Is(groupId), Arg.Any<CancellationToken>()).Returns(group);

        var updateConnectorCurrentInAmps =
            new SmartChargingApi.Groups.Features.UpdateConnectorCurrentInAmps.v1.UpdateConnectorCurrentInAmps(
                groupId.Value,
                chargeStationId.Value,
                connectorId.Value,
                newCurrent.Value
            );

        // Act
        await _handler.Handle(updateConnectorCurrentInAmps, CancellationToken.None);

        // Assert
        // Verify current was updated
        connectorToUpdate.MaxCurrentInAmps.ShouldBe(newCurrent);

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
        var connectorId = ConnectorId.Of(1);
        var newCurrent = CurrentInAmps.Of(25);

        _unitOfWorkMock
            .GroupRepository.GetByIdAsync(Arg.Is(groupId), Arg.Any<CancellationToken>())
            .Returns((Group)null!);

        var updateConnectorCurrentInAmps =
            new SmartChargingApi.Groups.Features.UpdateConnectorCurrentInAmps.v1.UpdateConnectorCurrentInAmps(
                groupId.Value,
                chargeStationId.Value,
                connectorId.Value,
                newCurrent.Value
            );

        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(() =>
            _handler.Handle(updateConnectorCurrentInAmps, CancellationToken.None)
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
        var connectorId = ConnectorId.Of(1);
        var newCurrent = CurrentInAmps.Of(25);

        var groupName = Name.Of("Test Group");
        var capacityInAmps = CurrentInAmps.Of(500);

        var group = Group.Create(groupId, groupName, capacityInAmps, chargeStation: null);

        _unitOfWorkMock.GroupRepository.GetByIdAsync(Arg.Is(groupId), Arg.Any<CancellationToken>()).Returns(group);

        var updateConnectorCurrentInAmps =
            new SmartChargingApi.Groups.Features.UpdateConnectorCurrentInAmps.v1.UpdateConnectorCurrentInAmps(
                groupId.Value,
                chargeStationId.Value,
                connectorId.Value,
                newCurrent.Value
            );

        // Act & Assert
        var exception = await Should.ThrowAsync<DomainException>(() =>
            _handler.Handle(updateConnectorCurrentInAmps, CancellationToken.None)
        );

        exception.Message.ShouldBe("Charge station not found");

        // Verify no repository update or commit
        _unitOfWorkMock.GroupRepository.DidNotReceiveWithAnyArgs().Update(default!);
        await _unitOfWorkMock.Received(0).CommitAsync(CancellationToken.None);

        // Verify no logging occurred
        _loggerMock.DidNotReceiveWithAnyArgs().LogInformation(default);
    }

    [Fact]
    public async Task Handle_WithNonExistentConnector_ThrowsDomainException()
    {
        // Arrange
        var groupId = GroupId.New();
        var chargeStationId = ChargeStationId.New();
        var connectorId = ConnectorId.Of(4);
        var newCurrent = CurrentInAmps.Of(5);

        var groupName = Name.Of("Test Group");
        var capacityInAmps = CurrentInAmps.Of(500);

        var chargeStation = new ChargeStationFake(2).Generate();
        chargeStation.Id = chargeStationId;

        var group = Group.Create(groupId, groupName, capacityInAmps, chargeStation);

        // Mock repository to return the group
        _unitOfWorkMock.GroupRepository.GetByIdAsync(Arg.Is(groupId), Arg.Any<CancellationToken>()).Returns(group);

        var updateConnectorCurrentInAmps =
            new SmartChargingApi.Groups.Features.UpdateConnectorCurrentInAmps.v1.UpdateConnectorCurrentInAmps(
                groupId.Value,
                chargeStationId.Value,
                connectorId.Value,
                newCurrent.Value
            );

        // Act & Assert
        var exception = await Should.ThrowAsync<DomainException>(() =>
            _handler.Handle(updateConnectorCurrentInAmps, CancellationToken.None)
        );

        exception.Message.ShouldBe($"Connector not found");

        // Verify no repository update or commit
        _unitOfWorkMock.GroupRepository.DidNotReceiveWithAnyArgs().Update(default!);
        await _unitOfWorkMock.Received(0).CommitAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_WithNullInput_ThrowsValidationException()
    {
        // Act & Assert
        await Should.ThrowAsync<ValidationException>(() => _handler.Handle(null!, CancellationToken.None));
    }
}
