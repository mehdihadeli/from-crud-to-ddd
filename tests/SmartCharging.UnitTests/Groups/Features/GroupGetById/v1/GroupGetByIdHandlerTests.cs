using Bogus;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SmartCharging.ServiceDefaults.Exceptions;
using SmartCharging.UnitTests.Groups.Mocks;
using SmartChargingApi.Groups.Contracts;
using SmartChargingApi.Groups.Dtos;
using SmartChargingApi.Groups.Features.GetGroupById.v1;
using SmartChargingApi.Groups.Models;
using SmartChargingApi.Groups.Models.ValueObjects;
using SmartChargingApi.Shared.Contracts;
using ValidationException = Bogus.ValidationException;

namespace SmartCharging.UnitTests.Groups.Features.GroupGetById.v1;

public class GetGroupByIdHandlerTests
{
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly IGroupStatisticsExternalProvider _statsProviderMock;
    private readonly ILogger<GetGroupByIdHandler> _loggerMock;
    private readonly GetGroupByIdHandler _handler;

    public GetGroupByIdHandlerTests()
    {
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _statsProviderMock = Substitute.For<IGroupStatisticsExternalProvider>();
        _loggerMock = Substitute.For<ILogger<GetGroupByIdHandler>>();

        _handler = new GetGroupByIdHandler(_unitOfWorkMock, _statsProviderMock, _loggerMock);
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

        // Mock external statistics provider
        _statsProviderMock
            .GetCapacityStatisticsAsync(group.Id.Value)
            .Returns(new GroupCapacityStatisticsDto(100, 200, 100));
        _statsProviderMock
            .GetEnergyConsumptionAsync(group.Id.Value)
            .Returns(new GroupEnergyConsumptionDto(1500, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow));

        var groupGetById = GetGroupById.Of(group.Id.Value);

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

        var groupGetById = GetGroupById.Of(nonExistentGroupId.Value);

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

        // Mock statistics provider (not actually used in assertion, but can be provided)
        _statsProviderMock.GetCapacityStatisticsAsync(group.Id.Value).Returns((GroupCapacityStatisticsDto?)null);
        _statsProviderMock.GetEnergyConsumptionAsync(group.Id.Value).Returns((GroupEnergyConsumptionDto?)null);

        var groupGetById = GetGroupById.Of(group.Id.Value);

        // Act
        await _handler.Handle(groupGetById, CancellationToken.None);

        // Assert
        await _unitOfWorkMock.GroupRepository.Received(1).GetByIdAsync(Arg.Is(group.Id), Arg.Any<CancellationToken>());
    }
}
