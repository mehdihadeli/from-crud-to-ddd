using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using SmartCharging.Groups.Contracts;
using SmartCharging.Groups.Features.CreateGroup.v1;
using SmartCharging.Groups.Models;
using SmartCharging.Groups.Models.ValueObjects;
using SmartCharging.Shared.BuildingBlocks.Exceptions;
using SmartCharging.UnitTests.Groups.Mocks;

namespace SmartCharging.UnitTests.Groups.Features.CreateGroup.v1;

public class CreateGroupHandlerTests
{
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly CreateGroupHandler _handler;
    private readonly ILogger<CreateGroupHandler> _loggerMock;

    public CreateGroupHandlerTests()
    {
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _loggerMock = Substitute.For<ILogger<CreateGroupHandler>>();
        _handler = new CreateGroupHandler(_unitOfWorkMock, _loggerMock);
    }

    [Fact]
    public async Task Handle_WithValidData_CreatesGroupSuccessfullyAndCommits()
    {
        // Arrange
        var groupName = Name.Of("Valid Group");
        var capacityInAmps = CurrentInAmps.Of(300);
        var chargeStation = new ChargeStationFake(numberOfConnectors: 3).Generate();

        var createGroup = new SmartCharging.Groups.Features.CreateGroup.v1.CreateGroup(
            Name: groupName,
            CapacityInAmps: capacityInAmps,
            ChargeStation: chargeStation
        );

        // Act
        var result = await _handler.Handle(createGroup, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.GroupId.ShouldBe(createGroup.GroupId.Value);

        // Verify repository interactions
        await _unitOfWorkMock
            .GroupRepository.Received(1)
            .AddAsync(
                Arg.Is<Group>(g =>
                    g.Name == groupName
                    && g.CapacityInAmps == capacityInAmps
                    && g.ChargeStations.Single() == chargeStation
                ),
                CancellationToken.None
            );

        // Verify commit
        await _unitOfWorkMock.Received(1).CommitAsync(CancellationToken.None);

        // Verify logging
        _loggerMock.Received(1);
    }

    [Fact]
    public async Task Handle_WhenCommitFails_ThrowsException()
    {
        // Arrange: Simulate failure during commit
        var groupName = Name.Of("Group with Commit Failure");
        var capacityInAmps = CurrentInAmps.Of(400);
        var chargeStation = new ChargeStationFake(numberOfConnectors: 1).Generate();

        var createGroup = new SmartCharging.Groups.Features.CreateGroup.v1.CreateGroup(
            Name: groupName,
            CapacityInAmps: capacityInAmps,
            ChargeStation: chargeStation
        );

        _unitOfWorkMock
            .When(repo => repo.CommitAsync(Arg.Any<CancellationToken>()))
            .Do(x => throw new Exception("Database commit failed."));

        // Act & Assert
        var exception = await Should.ThrowAsync<Exception>(() => _handler.Handle(createGroup, CancellationToken.None));

        exception.Message.ShouldBe("Database commit failed.");

        // Verify group was added but the commit failed
        _unitOfWorkMock.Received(1).GroupRepository.AddAsync(Arg.Any<Group>(), CancellationToken.None);
        _unitOfWorkMock.Received(1).CommitAsync(CancellationToken.None);

        // Verify logging
        _loggerMock.DidNotReceiveWithAnyArgs().LogInformation(default);
    }

    [Fact]
    public async Task Handle_WithNullRequest_ThrowsValidationException()
    {
        // Arrange: Pass null as request
        SmartCharging.Groups.Features.CreateGroup.v1.CreateGroup? nullRequest = null;

        // Act & Assert
        var exception = await Should.ThrowAsync<ValidationException>(() =>
            _handler.Handle(nullRequest!, CancellationToken.None)
        );

        exception.Message.ShouldBe("createGroup cannot be null or empty.");

        await _unitOfWorkMock.DidNotReceiveWithAnyArgs().GroupRepository.AddAsync(default!, default!);
        await _unitOfWorkMock.DidNotReceiveWithAnyArgs().CommitAsync(default);
        _loggerMock.DidNotReceiveWithAnyArgs().LogInformation(default);
    }
}
