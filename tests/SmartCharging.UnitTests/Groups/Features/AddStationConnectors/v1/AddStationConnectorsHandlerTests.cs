using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using SmartCharging.Groups.Contracts;
using SmartCharging.Groups.Dtos;
using SmartCharging.Groups.Features.AddStationConnectors.v1;
using SmartCharging.Groups.Models;
using SmartCharging.Groups.Models.ValueObjects;
using SmartCharging.Shared.Application.Data;
using SmartCharging.Shared.BuildingBlocks.Exceptions;

namespace SmartCharging.UnitTests.Groups.Features.AddStationConnectors.v1;

public class AddStationConnectorsHandlerTests
{
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly AddConnectorsHandler _handler;
    private readonly ILogger<AddConnectorsHandler> _loggerMock;

    public AddStationConnectorsHandlerTests()
    {
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _loggerMock = Substitute.For<ILogger<AddConnectorsHandler>>();
        _handler = new AddConnectorsHandler(_unitOfWorkMock, _loggerMock);
    }

    [Fact]
    public async Task Handle_WithValidData_AddsConnectorsSuccessfully()
    {
        // Arrange
        var groupId = GroupId.New();
        var chargeStationId = ChargeStationId.New();
        var groupCapacity = CurrentInAmps.Of(500);

        // Create a group with one charge station
        var chargeStation = ChargeStation.Create(
            chargeStationId,
            Name.Of("Station A"),
            new List<Connector>
            {
                Connector.Create(ConnectorId.Of(1), CurrentInAmps.Of(50), chargeStationId),
            }.AsReadOnly()
        );
        var group = Group.Create(groupId, Name.Of("Group A"), groupCapacity, chargeStation);

        // New connectors to be added
        var newConnectors = new List<ConnectorDto>
        {
            new(chargeStationId.Value, 2, 70),
            new(chargeStationId.Value, 3, 100),
        };

        _unitOfWorkMock.GroupRepository.GetByIdAsync(Arg.Is(groupId), Arg.Any<CancellationToken>()).Returns(group);

        var addConnectors = SmartCharging.Groups.Features.AddStationConnectors.v1.AddStationConnectors.Of(
            groupId.Value,
            chargeStationId.Value,
            newConnectors
        );

        // Act
        var result = await _handler.Handle(addConnectors, CancellationToken.None);

        // Assert
        result.Connectors.Count.ShouldBe(2);
        result.Connectors.ShouldContain(connector => connector.ConnectorId == 2 && connector.MaxCurrentInAmps == 70);

        // Verify repository updates
        _unitOfWorkMock.GroupRepository.Received(1).Update(group);
        await _unitOfWorkMock.Received(1).CommitAsync(CancellationToken.None);

        // Verify logging
        _loggerMock.Received(1);
    }

    [Fact]
    public async Task Handle_GroupNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var groupId = GroupId.New();
        var chargeStationId = ChargeStationId.New();

        _unitOfWorkMock
            .GroupRepository.GetByIdAsync(Arg.Is(groupId), Arg.Any<CancellationToken>())
            .Returns((Group)null!);

        var addConnectors = SmartCharging.Groups.Features.AddStationConnectors.v1.AddStationConnectors.Of(
            groupId.Value,
            chargeStationId.Value,
            new List<ConnectorDto> { new ConnectorDto(chargeStationId.Value, 1, 50) }
        );

        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(() =>
            _handler.Handle(addConnectors, CancellationToken.None)
        );

        exception.Message.ShouldBe($"Group with ID {groupId.Value} not found.");
        _unitOfWorkMock.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCommitFails_ThrowsException()
    {
        // Arrange
        var groupId = GroupId.New();
        var chargeStationId = ChargeStationId.New();
        var groupCapacity = CurrentInAmps.Of(300);

        // Add a charge station to the group with at least one connector
        var initialConnectors = new List<Connector>
        {
            Connector.Create(ConnectorId.Of(1), CurrentInAmps.Of(50), chargeStationId),
        }.AsReadOnly();

        var chargeStation = ChargeStation.Create(chargeStationId, Name.Of("Station A"), initialConnectors);

        var group = Group.Create(groupId, Name.Of("Group Commit Failure"), groupCapacity, chargeStation);

        // Mock repository to return the group
        _unitOfWorkMock.GroupRepository.GetByIdAsync(Arg.Is(groupId), Arg.Any<CancellationToken>()).Returns(group);

        // Ensure Commit fails
        _unitOfWorkMock
            .When(async x => await x.CommitAsync(Arg.Any<CancellationToken>()))
            .Do(x => throw new Exception("Commit failed."));

        // Prepare AddStationConnectors request with new connectors
        var addConnectors = SmartCharging.Groups.Features.AddStationConnectors.v1.AddStationConnectors.Of(
            groupId.Value,
            chargeStationId.Value,
            new List<ConnectorDto> { new ConnectorDto(chargeStationId.Value, 2, 50) }
        );

        // Act & Assert
        var exception = await Should.ThrowAsync<Exception>(() => _handler.Handle(addConnectors, CancellationToken.None)
        );

        // Verify the correct exception is thrown
        exception.Message.ShouldBe("Commit failed.");
    }
}
