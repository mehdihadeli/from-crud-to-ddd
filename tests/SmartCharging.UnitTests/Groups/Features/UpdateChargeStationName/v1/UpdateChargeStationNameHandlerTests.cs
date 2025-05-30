using Microsoft.Extensions.Logging;
using NSubstitute;
using SmartCharging.Groups.Contracts;
using SmartCharging.Groups.Features.UpdateChargeStationName.v1;
using SmartCharging.Groups.Models;
using SmartCharging.Groups.Models.ValueObjects;
using SmartCharging.Shared.BuildingBlocks.Exceptions;
using SmartCharging.UnitTests.Groups.Mocks;

namespace SmartCharging.UnitTests.Groups.Features.UpdateChargeStationName.v1;

public class UpdateChargeStationNameHandlerTests
{
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly ILogger<UpdateChargeStationNameHandler> _loggerMock;
    private readonly UpdateChargeStationNameHandler _handler;

    public UpdateChargeStationNameHandlerTests()
    {
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _loggerMock = Substitute.For<ILogger<UpdateChargeStationNameHandler>>();
        _handler = new UpdateChargeStationNameHandler(_unitOfWorkMock, _loggerMock);
    }

    [Fact]
    public async Task Handle_WithValidData_UpdatesChargeStationNameSuccessfullyAndCommits()
    {
        // Arrange
        var groupId = GroupId.New();
        var chargeStationId = ChargeStationId.New();
        var groupName = Name.Of("Test Group");
        var capacityInAmps = CurrentInAmps.Of(500);
        var newName = Name.Of("Updated Station Name");

        var chargeStation = new ChargeStationFake(2).Generate();
        chargeStation.Id = chargeStationId; // Ensure the ID matches

        var group = Group.Create(groupId, groupName, capacityInAmps, chargeStation);

        _unitOfWorkMock.GroupRepository.GetByIdAsync(Arg.Is(groupId), Arg.Any<CancellationToken>()).Returns(group);

        var updateChargeStationName =
            new SmartCharging.Groups.Features.UpdateChargeStationName.v1.UpdateChargeStationName(
                groupId,
                chargeStationId,
                newName
            );

        // Act
        await _handler.Handle(updateChargeStationName, CancellationToken.None);

        // Assert
        chargeStation.Name.ShouldBe(newName);

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
        var newName = Name.Of("Updated Station Name");

        _unitOfWorkMock
            .GroupRepository.GetByIdAsync(Arg.Is(groupId), Arg.Any<CancellationToken>())
            .Returns((Group)null!);

        var updateChargeStationName =
            new SmartCharging.Groups.Features.UpdateChargeStationName.v1.UpdateChargeStationName(
                groupId,
                chargeStationId,
                newName
            );

        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(() =>
            _handler.Handle(updateChargeStationName, CancellationToken.None)
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
        var capacityInAmps = CurrentInAmps.Of(500);
        var newName = Name.Of("Updated Station Name");

        var group = Group.Create(groupId, groupName, capacityInAmps, chargeStation: null);

        _unitOfWorkMock.GroupRepository.GetByIdAsync(Arg.Is(groupId), Arg.Any<CancellationToken>()).Returns(group);

        var updateChargeStationName =
            new SmartCharging.Groups.Features.UpdateChargeStationName.v1.UpdateChargeStationName(
                groupId,
                chargeStationId,
                newName
            );

        // Act & Assert
        var exception = await Should.ThrowAsync<DomainException>(() =>
            _handler.Handle(updateChargeStationName, CancellationToken.None)
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
