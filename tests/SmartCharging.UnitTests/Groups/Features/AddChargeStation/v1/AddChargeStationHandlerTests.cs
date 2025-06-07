using Microsoft.Extensions.Logging;
using NSubstitute;
using SmartCharging.Groups.Dtos;
using SmartCharging.Groups.Features.AddChargeStation.v1;
using SmartCharging.Groups.Models;
using SmartCharging.Groups.Models.ValueObjects;
using SmartCharging.Shared.Application.Data;
using SmartCharging.Shared.BuildingBlocks.Exceptions;

namespace SmartCharging.UnitTests.Groups.Features.AddChargeStation.v1;

public class AddChargeStationHandlerTests
{
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly AddChargeStationHandler _handler;
    private readonly ILogger<AddChargeStationHandler> _loggerMock;

    public AddChargeStationHandlerTests()
    {
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _loggerMock = Substitute.For<ILogger<AddChargeStationHandler>>();
        _handler = new AddChargeStationHandler(_unitOfWorkMock, _loggerMock);
    }

    [Fact]
    public async Task Handle_WithValidData_AddsChargeStationSuccessfullyAndCommits()
    {
        // Arrange
        var groupId = GroupId.New();
        var groupName = Name.Of("Test Group");
        var groupCapacity = CurrentInAmps.Of(300);

        // Create a group with no charge stations initially
        var group = Group.Create(groupId, groupName, groupCapacity, chargeStation: null);

        var chargeStationId = ChargeStationId.New();
        var chargeStationName = Name.Of("Station A");
        var connectors = new[]
        {
            Connector.Create(ConnectorId.Of(1), CurrentInAmps.Of(20), chargeStationId),
            Connector.Create(ConnectorId.Of(2), CurrentInAmps.Of(30), chargeStationId),
        }
            .ToList()
            .AsReadOnly();

        // Mock repository behavior
        _unitOfWorkMock.GroupRepository.GetByIdAsync(Arg.Is(group.Id), Arg.Any<CancellationToken>()).Returns(group);

        var addChargeStation = SmartCharging.Groups.Features.AddChargeStation.v1.AddChargeStation.Of(
            groupId.Value,
            chargeStationId.Value,
            chargeStationName.Value,
            connectors
                .Select(c => new ConnectorDto(c.ChargeStationId.Value, c.Id.Value, c.MaxCurrentInAmps.Value))
                .ToList()
        );

        // Act
        await _handler.Handle(addChargeStation, CancellationToken.None);

        // Assert
        group.ChargeStations.ShouldContain(station =>
            station.Id == ChargeStationId.Of(addChargeStation.ChargeStationId)
        );

        // Verify repository interactions
        _unitOfWorkMock.GroupRepository.Received(1).GetByIdAsync(group.Id, CancellationToken.None);
        _unitOfWorkMock.GroupRepository.Received(1).Update(group);
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
        var chargeStationName = Name.Of("Station A");
        var connectors = new[]
        {
            Connector.Create(ConnectorId.Of(1), CurrentInAmps.Of(20), chargeStationId),
            Connector.Create(ConnectorId.Of(2), CurrentInAmps.Of(30), chargeStationId),
        }
            .ToList()
            .AsReadOnly();

        var addChargeStation = SmartCharging.Groups.Features.AddChargeStation.v1.AddChargeStation.Of(
            groupId.Value,
            chargeStationId.Value,
            chargeStationName.Value,
            connectors
                .Select(c => new ConnectorDto(c.ChargeStationId.Value, c.Id.Value, c.MaxCurrentInAmps.Value))
                .ToList()
        );

        // Mock repository behavior to return null
        _unitOfWorkMock
            .GroupRepository.GetByIdAsync(Arg.Is(groupId), Arg.Any<CancellationToken>())
            .Returns((Group)null!);

        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(() =>
            _handler.Handle(addChargeStation, CancellationToken.None)
        );

        exception.Message.ShouldBe($"Group with ID {groupId.Value} not found.");

        // Verify repository interactions
        _unitOfWorkMock.GroupRepository.Received(1).GetByIdAsync(groupId, CancellationToken.None);
        _unitOfWorkMock.GroupRepository.DidNotReceive().Update(Arg.Any<Group>());
        await _unitOfWorkMock.DidNotReceive().CommitAsync(CancellationToken.None);

        // Verify no logging occurred
        _loggerMock.DidNotReceiveWithAnyArgs().LogInformation(default);
    }

    [Fact]
    public async Task Handle_WhenCommitFails_ThrowsException()
    {
        // Arrange
        var groupId = GroupId.New();
        var groupName = Name.Of("Test Group");
        var groupCapacity = CurrentInAmps.Of(300);

        var group = Group.Create(groupId, groupName, groupCapacity, chargeStation: null);

        var chargeStationId = ChargeStationId.New();
        var chargeStationName = Name.Of("Station A");
        var connectors = new[]
        {
            Connector.Create(ConnectorId.Of(1), CurrentInAmps.Of(20), chargeStationId),
            Connector.Create(ConnectorId.Of(2), CurrentInAmps.Of(30), chargeStationId),
        }
            .ToList()
            .AsReadOnly();

        _unitOfWorkMock.GroupRepository.GetByIdAsync(Arg.Is(group.Id), Arg.Any<CancellationToken>()).Returns(group);

        _unitOfWorkMock
            .When(repo => repo.CommitAsync(Arg.Any<CancellationToken>()))
            .Do(x => throw new Exception("Commit failed."));

        var addChargeStation = SmartCharging.Groups.Features.AddChargeStation.v1.AddChargeStation.Of(
            groupId.Value,
            chargeStationId.Value,
            chargeStationName.Value,
            connectors
                .Select(c => new ConnectorDto(c.ChargeStationId.Value, c.Id.Value, c.MaxCurrentInAmps.Value))
                .ToList()
        );

        // Act & Assert
        var exception = await Should.ThrowAsync<Exception>(() =>
            _handler.Handle(addChargeStation, CancellationToken.None)
        );

        exception.Message.ShouldBe("Commit failed.");

        // Verify repository interactions
        _unitOfWorkMock.GroupRepository.Received(1).Update(group);
        await _unitOfWorkMock.Received(1).CommitAsync(CancellationToken.None);

        // Verify no logging occurred
        _loggerMock.DidNotReceive().LogInformation(default);
    }

    [Fact]
    public async Task Handle_WithNullRequest_ThrowsValidationException()
    {
        // Act & Assert
        var exception = await Should.ThrowAsync<ValidationException>(() =>
            _handler.Handle(null!, CancellationToken.None)
        );

        exception.Message.ShouldBe("addChargeStation cannot be null or empty.");

        // Verify no repository interactions
        _unitOfWorkMock.GroupRepository.DidNotReceiveWithAnyArgs().GetByIdAsync(default!, default);
        await _unitOfWorkMock.DidNotReceive().CommitAsync(CancellationToken.None);

        // Verify no logging occurred
        _loggerMock.DidNotReceive().LogInformation(default);
    }
}
