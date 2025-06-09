using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using SmartCharging.EndToEndTests.Group.Mocks;
using SmartCharging.Groups.Dtos;
using SmartCharging.Groups.Features.GetGroupsByPage.v1;
using SmartCharging.Shared.Application.Data;
using SmartCharging.TestsShared.Fixtures;
using Xunit.Abstractions;

namespace SmartCharging.EndToEndTests.Group.Features.GetGroupsByPage.v1;

public class GetGroupsByPageTests(
    SharedFixture<SmartChargingMetadata, SmartChargingDbContext> sharedFixture,
    ITestOutputHelper outputHelper
) : SmartChargingEndToEndTestBase(sharedFixture, outputHelper)
{
    [Fact]
    internal async Task GetGroupsByPage_WithValidPageRequest_Should_ReturnPaginatedGroups()
    {
        // Arrange
        var groupsToCreate = Enumerable
            .Range(1, 15)
            .Select(_ => new GroupFake(numberOfConnectorsPerStation: 2).Generate())
            .ToList();

        await SharedFixture.ExecuteEfDbContextAsync(async db =>
        {
            await db.Groups.AddRangeAsync(groupsToCreate);
            await db.SaveChangesAsync();
        });

        // Act
        var pageSize = 5;
        var pageNumber = 1;
        var getGroupsRoute = $"{Constants.Routes.Groups.GetByPage}?pageSize={pageSize}&pageNumber={pageNumber}";

        var getGroupsResponse = await SharedFixture.GuestClient.GetFromJsonAsync<GetGroupsByPageResponse>(
            getGroupsRoute
        );

        // Assert
        getGroupsResponse.ShouldNotBeNull();
        getGroupsResponse.Groups.ShouldNotBeNull();
        getGroupsResponse.Groups.Count.ShouldBe(pageSize);
        getGroupsResponse.TotalCount.ShouldBe(15);

        // Verify all items in the page match
        foreach (var responseGroupDto in getGroupsResponse.Groups)
        {
            var matchingGroup = groupsToCreate.FirstOrDefault(g => g.Id.Value == responseGroupDto.GroupId);
            matchingGroup.ShouldNotBeNull();
            responseGroupDto.Name.ShouldBe(matchingGroup.Name.Value);
            responseGroupDto.GroupId.ShouldBe(matchingGroup.Id.Value);
        }
    }

    [Fact]
    internal async Task GetGroupsByPage_WithEmptyDatabase_Should_ReturnEmptyList()
    {
        // Arrange
        await SharedFixture.ExecuteEfDbContextAsync(async db =>
        {
            db.Groups.RemoveRange(db.Groups);
            await db.SaveChangesAsync();
        });

        // Act
        var pageSize = 5;
        var pageNumber = 1;
        var getGroupsRoute = $"{Constants.Routes.Groups.GetByPage}?pageSize={pageSize}&pageNumber={pageNumber}";

        var getGroupsResponse = await SharedFixture.GuestClient.GetFromJsonAsync<GetGroupsByPageResponse>(
            getGroupsRoute
        );

        // Assert
        getGroupsResponse.ShouldNotBeNull();
        getGroupsResponse.Groups.ShouldNotBeNull();
        getGroupsResponse.Groups.ShouldBeEmpty();
        getGroupsResponse.TotalCount.ShouldBe(0);
    }

    [Fact]
    internal async Task GetGroupsByPage_WithInvalidPageSize_Should_ReturnBadRequest()
    {
        // Arrange
        var invalidPageSize = -5;
        var getGroupsRoute = $"{Constants.Routes.Groups.GetByPage}?pageSize={invalidPageSize}&pageNumber=1";

        // Act
        var response = await SharedFixture.GuestClient.GetAsync(getGroupsRoute);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
