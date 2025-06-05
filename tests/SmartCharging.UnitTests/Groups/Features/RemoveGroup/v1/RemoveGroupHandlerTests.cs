using Microsoft.Extensions.Logging;
using NSubstitute;
using SmartCharging.Groups.Contracts;
using SmartCharging.Groups.Features.RemoveGroup.v1;
using SmartCharging.Groups.Models;
using SmartCharging.Groups.Models.ValueObjects;
using SmartCharging.Shared.Application.Data;
using SmartCharging.Shared.BuildingBlocks.Exceptions;
using SmartCharging.UnitTests.Groups.Mocks;

namespace SmartCharging.UnitTests.Groups.Features.RemoveGroup.v1;

public class RemoveGroupHandlerTests
{
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly ILogger<RemoveGroupHandler> _loggerMock;
    private readonly RemoveGroupHandler _handler;

    public RemoveGroupHandlerTests()
    {
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _loggerMock = Substitute.For<ILogger<RemoveGroupHandler>>();
        _handler = new RemoveGroupHandler(_unitOfWorkMock, _loggerMock);
    }

    [Fact]
    public async Task Handle_WithValidGroup_RemovesGroupSuccessfullyAndCommits()
    {
        // Arrange
        var groupId = GroupId.New();
        var groupName = Name.Of("Test Group");
        var capacityInAmps = CurrentInAmps.Of(300);

        var chargeStation = new ChargeStationFake(1).Generate();
        var group = Group.Create(groupId, groupName, capacityInAmps, chargeStation);

        _unitOfWorkMock.GroupRepository.GetByIdAsync(Arg.Is(groupId), Arg.Any<CancellationToken>()).Returns(group);

        var removeGroup = SmartCharging.Groups.Features.RemoveGroup.v1.RemoveGroup.Of(groupId.Value);

        // Act
        await _handler.Handle(removeGroup, CancellationToken.None);

        // Assert
        // Verify the group was removed
        _unitOfWorkMock.GroupRepository.Received(1).Remove(Arg.Is(group));
        await _unitOfWorkMock.Received(1).CommitAsync(CancellationToken.None);

        // Verify logging
        _loggerMock.Received(1);
    }

    [Fact]
    public async Task Handle_WithNonExistentGroup_ThrowsNotFoundException()
    {
        // Arrange
        var groupId = GroupId.New();

        _unitOfWorkMock
            .GroupRepository.GetByIdAsync(Arg.Is(groupId), Arg.Any<CancellationToken>())
            .Returns((Group)null!);

        var removeGroup = SmartCharging.Groups.Features.RemoveGroup.v1.RemoveGroup.Of(groupId.Value);

        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(() =>
            _handler.Handle(removeGroup, CancellationToken.None)
        );

        exception.Message.ShouldBe($"Group with ID {groupId.Value} not found.");

        // Verify no attempt to remove or commit
        _unitOfWorkMock.GroupRepository.DidNotReceiveWithAnyArgs().Remove(default!);
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
