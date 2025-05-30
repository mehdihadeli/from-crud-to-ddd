using System.Net;
using Microsoft.EntityFrameworkCore;
using SmartCharging.EndToEndTests.Group.Mocks;
using SmartCharging.Shared.Application.Data;
using SmartCharging.TestsShared.Fixtures;
using Xunit.Abstractions;

namespace SmartCharging.EndToEndTests.Group.Features.RemoveGroup.v1;

public class RemoveGroupTests(
    SharedFixture<SmartChargingMetadata, SmartChargingDbContext> sharedFixture,
    ITestOutputHelper outputHelper
) : SmartChargingEndToEndTestBase(sharedFixture, outputHelper)
{
    [Fact]
    internal async Task RemoveGroup_WithValidGroupId_Should_RemoveGroupSuccessfully()
    {
        // Arrange
        var fakeGroup = new GroupFake(numberOfConnectorsPerStation: 2).Generate();
        await SharedFixture.ExecuteEfDbContextAsync(async db =>
        {
            await db.Groups.AddAsync(fakeGroup);
            await db.SaveChangesAsync();
        });

        var groupId = fakeGroup.Id.Value;
        var deleteGroupRoute = Constants.Routes.Groups.Delete(groupId);

        // Act
        var deleteResponse = await SharedFixture.GuestClient.DeleteAsync(deleteGroupRoute);

        // Assert
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var groupExists = await SharedFixture.ExecuteEfDbContextAsync(async db =>
        {
            return await db.Groups.AnyAsync(g => g.Id == fakeGroup.Id);
        });

        groupExists.ShouldBeFalse();
    }

    [Fact]
    internal async Task RemoveGroup_WithNonExistentGroupId_Should_ReturnNotFound()
    {
        // Arrange
        var nonExistentGroupId = Guid.NewGuid();
        var deleteGroupRoute = Constants.Routes.Groups.Delete(nonExistentGroupId);

        // Act
        var deleteResponse = await SharedFixture.GuestClient.DeleteAsync(deleteGroupRoute);

        // Assert
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
