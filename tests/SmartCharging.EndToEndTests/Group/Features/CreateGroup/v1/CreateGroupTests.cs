using System.Net;
using System.Net.Http.Json;
using SmartCharging.EndToEndTests.Group.Mocks;
using SmartCharging.ServiceDefaults.Extensions;
using SmartCharging.Shared.Data;
using SmartCharging.TestsShared.Fixtures;
using SmartChargingApi;
using SmartChargingApi.Groups.Features.CreateGroup.v1;
using SmartChargingApi.Shared.Data;

namespace SmartCharging.EndToEndTests.Group.Features.CreateGroup.v1;

//TestName: `MethodName_Condition_ExpectedResult`
public class CreateGroupTests(SharedFixture<SmartChargingMetadata, SmartChargingDbContext> sharedFixture)
    : SmartChargingEndToEndTestBase(sharedFixture)
{
    [Fact]
    internal async Task CreateGroup_WhenRequestIsValid_ReturnsCreatedStatus()
    {
        // Arrange
        var createGroupRequestFake = new CreateGroupRequestFake().Generate();
        var route = Constants.Routes.Groups.Create;

        // Act
        var response = await SharedFixture.GuestClient.PostAsJsonAsync(
            route,
            createGroupRequestFake,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    internal async Task CreateGroup_WhenRequestIsValid_ReturnsValidGroupResponse()
    {
        // Arrange
        var createGroupRequestFake = new CreateGroupRequestFake().Generate();
        var route = Constants.Routes.Groups.Create;

        // Act
        var response = await SharedFixture.GuestClient.PostAsJsonAsync(
            route,
            createGroupRequestFake,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadFromJsonAsync<CreateGroupResponse>(
            cancellationToken: TestContext.Current.CancellationToken
        );
        responseContent.ShouldNotBeNull();
        responseContent.GroupId.NotBeEmpty();
    }
}
