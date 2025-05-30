using System.Net;
using System.Net.Http.Json;
using SmartCharging.EndToEndTests.Group.Mocks;
using SmartCharging.Groups.Features.CreateGroup.v1;
using SmartCharging.Shared.Application.Data;
using SmartCharging.Shared.BuildingBlocks.Extensions;
using SmartCharging.TestsShared.Fixtures;
using Xunit.Abstractions;

namespace SmartCharging.EndToEndTests.Group.Features.CreateGroup.v1;

public class CreateGroupTests(
    SharedFixture<SmartChargingMetadata, SmartChargingDbContext> sharedFixture,
    ITestOutputHelper outputHelper
) : SmartChargingEndToEndTestBase(sharedFixture, outputHelper)
{
    [Fact]
    internal async Task AddGroupEndpoint_WithValidRequest_Should_ReturnCreatedStatus()
    {
        // Arrange
        var createGroupRequestFake = new CreateGroupRequestFake().Generate();
        var route = Constants.Routes.Groups.Create;

        // Act
        var response = await SharedFixture.GuestClient.PostAsJsonAsync(route, createGroupRequestFake);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    internal async Task AddGroupEndpoint_WithValidRequest_Should_ReturnValidResponse()
    {
        // Arrange
        var createGroupRequestFake = new CreateGroupRequestFake().Generate();
        var route = Constants.Routes.Groups.Create;

        // Act
        var response = await SharedFixture.GuestClient.PostAsJsonAsync(route, createGroupRequestFake);

        // Assert
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadFromJsonAsync<CreateGroupResponse>();
        responseContent.ShouldNotBeNull();
        responseContent.GroupId.NotBeEmpty();
    }
}
