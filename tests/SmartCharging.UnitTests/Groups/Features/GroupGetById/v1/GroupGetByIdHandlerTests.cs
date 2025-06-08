using Bogus;
using NSubstitute;
using Shouldly;
using SmartCharging.Groups.Contracts;
using SmartCharging.Groups.Features.GroupGetById.v1;
using SmartCharging.Groups.Models;
using SmartCharging.Groups.Models.ValueObjects;
using SmartCharging.Shared.Application.Contracts;
using SmartCharging.Shared.Application.Data;
using SmartCharging.Shared.BuildingBlocks.Exceptions;
using SmartCharging.UnitTests.Groups.Mocks;
using ValidationException = SmartCharging.Shared.BuildingBlocks.Exceptions.ValidationException;

namespace SmartCharging.UnitTests.Groups.Features.GroupGetById.v1;

public class GroupGetByIdHandlerTests
{
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly GroupGetByIdHandler _handler;

    public GroupGetByIdHandlerTests()
    {
        // Mock the UnitOfWork and its GroupRepository
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _handler = new GroupGetByIdHandler(_unitOfWorkMock);
    }

    [Fact]
    public async Task Handle_WithValidGroupId_ReturnsGroup()
    {
        // Arrange
        var groupFake = new GroupFake(
            numberOfConnectorsPerStation: 3,
            groupCapacity: new Faker().PickRandom(new List<int?> { 400, 450, 500, 550 }),
            maxConnectorCurrent: new Faker().PickRandom(new List<int?> { 50, 55, 60, 65 })
        );
        var group = groupFake.Generate();

        // Mock GetByIdAsync to return the fake group
        _unitOfWorkMock.GroupRepository.GetByIdAsync(Arg.Is(group.Id), Arg.Any<CancellationToken>()).Returns(group);

        var groupGetById = SmartCharging.Groups.Features.GroupGetById.v1.GroupGetById.Of(group.Id.Value);

        // Act
        var result = await _handler.Handle(groupGetById, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Group.Name.ShouldBe(group.Name.Value);
        result.Group.CapacityInAmps.ShouldBe(group.CapacityInAmps.Value);
        result.Group.ChargeStations.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Handle_WithNonExistentGroupId_ThrowsNotFoundException()
    {
        // Arrange
        var nonExistentGroupId = GroupId.New();

        // Mock GetByIdAsync to return null
        _unitOfWorkMock
            .GroupRepository.GetByIdAsync(Arg.Is(nonExistentGroupId), Arg.Any<CancellationToken>())
            .Returns((Group)null!);

        var groupGetById = SmartCharging.Groups.Features.GroupGetById.v1.GroupGetById.Of(nonExistentGroupId.Value);

        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(() =>
            _handler.Handle(groupGetById, CancellationToken.None)
        );

        exception.Message.ShouldBe($"Group with ID {nonExistentGroupId.Value} not found.");
    }

    [Fact]
    public void Handle_WithNullInput_ThrowsValidationException()
    {
        // Act & Assert
        Should.ThrowAsync<ValidationException>(() => _handler.Handle(null!, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_PassesCorrectIdToRepository()
    {
        // Arrange
        var groupFake = new GroupFake(
            numberOfConnectorsPerStation: 3,
            groupCapacity: new Faker().PickRandom(new List<int?> { 400, 450, 500, 550 }),
            maxConnectorCurrent: new Faker().PickRandom(new List<int?> { 50, 55, 60, 65 })
        );
        var group = groupFake.Generate();

        // Mock GetByIdAsync to return the fake group
        _unitOfWorkMock.GroupRepository.GetByIdAsync(Arg.Is(group.Id), Arg.Any<CancellationToken>()).Returns(group);

        var groupGetById = SmartCharging.Groups.Features.GroupGetById.v1.GroupGetById.Of(group.Id.Value);

        // Act
        await _handler.Handle(groupGetById, CancellationToken.None);

        // Assert
        await _unitOfWorkMock.GroupRepository.Received(1).GetByIdAsync(Arg.Is(group.Id), Arg.Any<CancellationToken>());
    }
}
