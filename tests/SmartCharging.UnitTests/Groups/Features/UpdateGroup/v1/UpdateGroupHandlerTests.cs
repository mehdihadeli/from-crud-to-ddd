using Microsoft.Extensions.Logging;
using NSubstitute;
using SmartCharging.Groups.Contracts;
using SmartCharging.Groups.Features.UpdateGroup.v1;
using SmartCharging.Groups.Models;
using SmartCharging.Groups.Models.ValueObjects;
using SmartCharging.Shared.BuildingBlocks.Exceptions;

namespace SmartCharging.UnitTests.Groups.Features.UpdateGroup.v1;

public class UpdateGroupHandlerTests
{
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly ILogger<UpdateGroupHandler> _loggerMock;
    private readonly UpdateGroupHandler _handler;

    public UpdateGroupHandlerTests()
    {
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _loggerMock = Substitute.For<ILogger<UpdateGroupHandler>>();
        _handler = new UpdateGroupHandler(_unitOfWorkMock, _loggerMock);
    }

    [Fact]
    public async Task Handle_WithValidData_UpdatesGroupSuccessfullyAndCommits()
    {
        // Arrange
        var groupId = GroupId.New();
        var newName = Name.Of("Updated Group Name");
        var newCapacity = CurrentInAmps.Of(400);

        var groupName = Name.Of("Original Group Name");
        var capacityInAmps = CurrentInAmps.Of(300);

        var group = Group.Create(groupId, groupName, capacityInAmps, chargeStation: null);

        _unitOfWorkMock.GroupRepository.GetByIdAsync(Arg.Is(groupId), Arg.Any<CancellationToken>()).Returns(group);

        var updateGroup = new SmartCharging.Groups.Features.UpdateGroup.v1.UpdateGroup(
            groupId.Value,
            newName.Value,
            newCapacity.Value
        );

        // Act
        await _handler.Handle(updateGroup, CancellationToken.None);

        // Assert
        group.Name.ShouldBe(newName);
        group.CapacityInAmps.ShouldBe(newCapacity);

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
        var newName = Name.Of("Updated Group Name");
        var newCapacity = CurrentInAmps.Of(400);

        // Mock repository to return null (group does not exist)
        _unitOfWorkMock
            .GroupRepository.GetByIdAsync(Arg.Is(groupId), Arg.Any<CancellationToken>())
            .Returns((Group)null!);

        var updateGroup = new SmartCharging.Groups.Features.UpdateGroup.v1.UpdateGroup(
            groupId.Value,
            newName.Value,
            newCapacity.Value
        );

        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(() =>
            _handler.Handle(updateGroup, CancellationToken.None)
        );

        exception.Message.ShouldBe($"Group with ID {groupId.Value} not found.");

        // Verify no repository update or commit
        _unitOfWorkMock.GroupRepository.DidNotReceiveWithAnyArgs().Update(default!);
        await _unitOfWorkMock.Received(0).CommitAsync(CancellationToken.None);

        // Verify no logging occurred
        _loggerMock.DidNotReceiveWithAnyArgs().LogInformation(default);
    }

    [Fact]
    public async Task Handle_WithInvalidInput_ThrowsValidationException()
    {
        // Act & Assert
        await Should.ThrowAsync<ValidationException>(() => _handler.Handle(null!, CancellationToken.None));
    }
}
