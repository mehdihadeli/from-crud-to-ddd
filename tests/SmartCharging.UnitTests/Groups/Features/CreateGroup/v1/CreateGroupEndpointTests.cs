using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SmartCharging.ServiceDefaults.Exceptions;
using SmartChargingApi.Groups.Contracts;
using SmartChargingApi.Groups.Features.CreateGroup.v1;
using SmartChargingApi.Groups.Features.GetGroupById.v1;
using SmartChargingApi.Groups.Models;
using SmartChargingApi.Shared.Contracts;

namespace SmartCharging.UnitTests.Groups.Features.CreateGroup.v1;

public class CreateGroupEndpointTests
{
    [Fact]
    public async Task HandleAsync_Returns_CreatedAtRoute_WithValidData_When_Successful()
    {
        // Arrange
        var request = new CreateGroupRequest(
            "EV Fleet",
            250,
            new CreateGroupRequest.CreateChargeStationRequest(
                "Station Alpha",
                new List<CreateGroupRequest.CreateConnectorRequest> { new(1, 50), new(2, 50) }
            )
        );

        var mockRepo = Substitute.For<IGroupRepository>();
        mockRepo
            .AddAsync(
                Arg.Is<Group>(x =>
                    x.Name.Value == request.Name
                    && x.CapacityInAmps.Value == request.CapacityInAmps
                    && x.ChargeStations.First().Name.Value == request.ChargeStationRequest!.Name
                ),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.CompletedTask);

        var mockUow = Substitute.For<IUnitOfWork>();
        mockUow.GroupRepository.Returns(mockRepo);
        mockUow.CommitAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(1));

        var logger = Substitute.For<ILogger<CreateGroupHandler>>();
        var handler = new CreateGroupHandler(mockUow, logger);

        var parameters = new CreateGroupRequestParameters(request, handler, CancellationToken.None);

        // Act
        var result = await InvokeHandleAsync(parameters);

        // Assert
        var createdRoute = result.Result.ShouldBeOfType<CreatedAtRoute<CreateGroupResponse>>();
        createdRoute.RouteName.ShouldBe(nameof(GetGroupById));
        createdRoute.StatusCode.ShouldBe(StatusCodes.Status201Created);
        createdRoute.RouteValues.Values.First().ShouldNotBe(Guid.Empty);
        var createGroupResponse = createdRoute.Value.ShouldNotBeNull().ShouldBeOfType<CreateGroupResponse>();
        createGroupResponse.GroupId.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task HandleAsync_WithoutChargeStation_ReturnsCreatedAtRoute()
    {
        // No ChargeStation
        var request = new CreateGroupRequest("SoloGroup", 120);

        var mockRepo = Substitute.For<IGroupRepository>();
        mockRepo
            .AddAsync(
                Arg.Is<Group>(x => x.Name.Value == request.Name && x.CapacityInAmps.Value == request.CapacityInAmps),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.CompletedTask);

        var mockUow = Substitute.For<IUnitOfWork>();
        mockUow.GroupRepository.Returns(mockRepo);
        mockUow.CommitAsync(Arg.Any<CancellationToken>()).Returns(1);

        var logger = Substitute.For<ILogger<CreateGroupHandler>>();
        var handler = new CreateGroupHandler(mockUow, logger);
        var parameters = new CreateGroupRequestParameters(request, handler, CancellationToken.None);

        var result = await InvokeHandleAsync(parameters);

        var createdRoute = result.Result.ShouldBeOfType<CreatedAtRoute<CreateGroupResponse>>();
        createdRoute.RouteName.ShouldBe(nameof(GetGroupById));
        createdRoute.StatusCode.ShouldBe(StatusCodes.Status201Created);
        var createGroupResponse = createdRoute.Value.ShouldNotBeNull().ShouldBeOfType<CreateGroupResponse>();
        createGroupResponse.GroupId.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task HandleAsync_WithNullRequest_Should_Throw_ValidationException()
    {
        // Arrange
        var mockUow = Substitute.For<IUnitOfWork>();
        var logger = Substitute.For<ILogger<CreateGroupHandler>>();
        var handler = new CreateGroupHandler(mockUow, logger);
        var parameters = new CreateGroupRequestParameters(null, handler, CancellationToken.None);

        // Act
        var act = async () => await InvokeHandleAsync(parameters);

        // Assert
        // during runtime with having problem details middleware and our ProblemDetailsExceptionHandler the exception will convert to a problem detail in the response
        var exception = await act.ShouldThrowAsync<ValidationException>();
        exception.Message.ShouldBe("name cannot be null or empty.");
    }

    [Fact]
    public async Task HandleAsync_WithNegativeCapacity_Should_Throw_ValidationException()
    {
        var request = new CreateGroupRequest("InvalidGroup", -100);

        var mockUow = Substitute.For<IUnitOfWork>();
        var logger = Substitute.For<ILogger<CreateGroupHandler>>();
        var handler = new CreateGroupHandler(mockUow, logger);
        var parameters = new CreateGroupRequestParameters(request, handler, CancellationToken.None);

        var act = async () => await InvokeHandleAsync(parameters);

        // Assert
        // during runtime with having problem details middleware and our ProblemDetailsExceptionHandler the exception will convert to a problem detail in the response
        var exception = await act.ShouldThrowAsync<ValidationException>();
        exception.Message.ShouldBe("capacityInAmps cannot be negative or zero.");
    }

    [Fact]
    public async Task HandleAsync_WhenCommitFails_ReturnsProblem()
    {
        var request = new CreateGroupRequest("TestGroup", 150);

        var mockRepo = Substitute.For<IGroupRepository>();
        var mockUow = Substitute.For<IUnitOfWork>();
        mockUow.GroupRepository.Returns(mockRepo);
        mockUow.CommitAsync(Arg.Any<CancellationToken>()).Throws(new Exception("Commit failure"));

        var logger = Substitute.For<ILogger<CreateGroupHandler>>();
        var handler = new CreateGroupHandler(mockUow, logger);
        var parameters = new CreateGroupRequestParameters(request, handler, CancellationToken.None);

        var act = async () => await InvokeHandleAsync(parameters);

        // Assert
        // during runtime with having problem details middleware and our ProblemDetailsExceptionHandler the exception will convert to a problem detail in the response
        var exception = await act.ShouldThrowAsync<Exception>();
        exception.Message.ShouldBe("Commit failure");
    }

    private static async Task<INestedHttpResult> InvokeHandleAsync(CreateGroupRequestParameters parameters)
    {
        var method = typeof(CreateGroupEndpoint).GetMethod(
            "HandleAsync",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
        )!;
        dynamic task = method.Invoke(null, new object[] { parameters })!;

        return await task;
    }
}
