using Microsoft.Extensions.Logging;
using NSubstitute;
using SmartCharging.ServiceDefaults.Exceptions;
using SmartCharging.UnitTests.Groups.Mocks;
using SmartChargingApi.Groups.Features.RemoveChargeStation.v1;
using SmartChargingApi.Groups.Models;
using SmartChargingApi.Groups.Models.ValueObjects;
using SmartChargingApi.Shared.Contracts;

namespace SmartCharging.UnitTests.Groups.Features.RemoveChargeStation.v1;

public class RemoveChargeStationHandlerTests
{
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly ILogger<RemoveChargeStationHandler> _loggerMock;
    private readonly RemoveChargeStationHandler _handler;

    public RemoveChargeStationHandlerTests()
    {
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _loggerMock = Substitute.For<ILogger<RemoveChargeStationHandler>>();
        _handler = new RemoveChargeStationHandler(_unitOfWorkMock, _loggerMock);
    }

    [Fact]
    public async Task Handle_WithValidData_RemovesChargeStationSuccessfullyAndCommits()
    {
        // Arrange
        var groupId = GroupId.New();
        var groupName = Name.Of("Test Group");
        var capacityInAmps = CurrentInAmps.Of(300);

        var chargeStation = new ChargeStationFake(1).Generate();

        var group = Group.Create(groupId, groupName, capacityInAmps, chargeStation);

        _unitOfWorkMock.GroupRepository.GetByIdAsync(Arg.Is(groupId), Arg.Any<CancellationToken>()).Returns(group);

        var removeChargeStation = SmartChargingApi.Groups.Features.RemoveChargeStation.v1.RemoveChargeStation.Of(
            groupId.Value,
            chargeStation.Id.Value
        );

        // Act
        await _handler.Handle(removeChargeStation, CancellationToken.None);

        // Assert
        group.ChargeStations.ShouldBeEmpty();

        // Verify repository interactions
        _unitOfWorkMock
            .GroupRepository.Received(1)
            .Update(Arg.Is<Group>(g => g.Id == groupId && g.ChargeStations.Count == 0));
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

        _unitOfWorkMock
            .GroupRepository.GetByIdAsync(Arg.Is(groupId), Arg.Any<CancellationToken>())
            .Returns((Group)null!);

        var removeChargeStation = SmartChargingApi.Groups.Features.RemoveChargeStation.v1.RemoveChargeStation.Of(
            groupId.Value,
            chargeStationId.Value
        );

        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(() =>
            _handler.Handle(removeChargeStation, CancellationToken.None)
        );

        exception.Message.ShouldBe($"Group with ID {groupId.Value} not found.");

        // Verify no updates or commits were attempted
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

        var removeChargeStation = SmartChargingApi.Groups.Features.RemoveChargeStation.v1.RemoveChargeStation.Of(
            groupId.Value,
            chargeStationId.Value
        );

        // Act & Assert
        var exception = await Should.ThrowAsync<DomainException>(() =>
            _handler.Handle(removeChargeStation, CancellationToken.None)
        );

        exception.Message.ShouldBe("Charge station not found in this group");

        // Verify no update or commit was performed
        _unitOfWorkMock.GroupRepository.DidNotReceiveWithAnyArgs().Update(default!);
        await _unitOfWorkMock.Received(0).CommitAsync(CancellationToken.None);

        // Verify no logging occurred
        _loggerMock.DidNotReceiveWithAnyArgs().LogInformation(default);
    }

    [Fact]
    public void Handle_WithNullInput_ThrowsValidationException()
    {
        // Act & Assert
        Should.ThrowAsync<ValidationException>(() => _handler.Handle(null!, CancellationToken.None));
    }
}
