using Microsoft.Extensions.DependencyInjection;
using SmartCharging.IntegrationTests.Groups.Mocks;
using SmartCharging.Shared.Data;
using SmartCharging.TestsShared.Fixtures;
using SmartChargingApi;
using SmartChargingApi.Groups.Features.GetGroupsByPage.v1;
using SmartChargingApi.Shared.Data;

namespace SmartCharging.IntegrationTests.Groups.Features.GetGroupsByPage.v1;

public class GetGroupsByPageTests(SharedFixture<SmartChargingMetadata, SmartChargingDbContext> sharedFixture)
    : SmartChargingIntegrationTestBase(sharedFixture)
{
    [Fact]
    internal async Task GetGroupsByPageAsync_WhenDataIsValid_ShouldReturnExistingGroups()
    {
        // Arrange
        var fakeGroups = new GroupFake().Generate(10);
        await SharedFixture.InsertEfDbContextAsync(fakeGroups.ToArray());

        // Act
        // Request page 1 with 5 groups per page
        var getGroupsByPage = new SmartChargingApi.Groups.Features.GetGroupsByPage.v1.GetGroupsByPage(1, 5);
        var handler = Scope.ServiceProvider.GetRequiredService<GetGroupsByPageHandler>();
        var result = await handler.Handle(getGroupsByPage, CancellationToken.None);

        // Assert
        result.Groups.ShouldNotBeNull();
        result.Groups.ShouldNotBeEmpty();
        result.Groups.Count.ShouldBe(5);
        result.PageSize.ShouldBe(5);
        result.PageCount.ShouldBe(2);
        result.TotalCount.ShouldBe(10);
    }

    [Fact]
    internal async Task GetGroupsByPageAsync_WhenGroupsExist_ShouldReturnCorrectlyMappedGroups()
    {
        // Arrange
        var fakeGroups = new GroupFake().Generate(10).ToList();

        await SharedFixture.InsertEfDbContextAsync(fakeGroups.ToArray());

        var getGroupsByPage = new SmartChargingApi.Groups.Features.GetGroupsByPage.v1.GetGroupsByPage(1, 5);
        var handler = Scope.ServiceProvider.GetRequiredService<GetGroupsByPageHandler>();

        // Act
        var result = await handler.Handle(getGroupsByPage, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Groups.ShouldNotBeNull();
        result.Groups.Count.ShouldBe(5);
        result.TotalCount.ShouldBe(10);

        // Verify all items in the page match
        foreach (var responseGroupDto in result.Groups)
        {
            var matchingGroup = fakeGroups.FirstOrDefault(g => g.Id.Value == responseGroupDto.GroupId);
            matchingGroup.ShouldNotBeNull();
            responseGroupDto.Name.ShouldBe(matchingGroup.Name.Value);
            responseGroupDto.GroupId.ShouldBe(matchingGroup.Id.Value);
        }
    }

    [Fact]
    internal async Task GetGroupsByPageAsync_WhenDatabaseIsEmpty_ShouldReturnEmptyPagedResult()
    {
        // Act
        var getGroupsByPage = new SmartChargingApi.Groups.Features.GetGroupsByPage.v1.GetGroupsByPage(1, 5); // Test on an empty database.
        var handler = Scope.ServiceProvider.GetRequiredService<GetGroupsByPageHandler>();
        var result = await handler.Handle(getGroupsByPage, CancellationToken.None);

        // Assert
        result.Groups.ShouldNotBeNull();
        result.Groups.ShouldBeEmpty();
        result.PageSize.ShouldBe(5);
        result.PageCount.ShouldBe(0);
        result.TotalCount.ShouldBe(0);
    }

    [Theory]
    [InlineData(3, 3, 1)]
    [InlineData(10, 5, 2)]
    [InlineData(10, 1, 10)]
    internal async Task GetGroupsByPageAsync_WhenCustomPageSizeIsUsed_ShouldReturnCorrectPagination(
        int totalGroups,
        int pageSize,
        int expectedPageCount
    )
    {
        // Arrange
        var fakeGroups = new GroupFake().Generate(totalGroups);
        await SharedFixture.InsertEfDbContextAsync(fakeGroups.ToArray());

        // Act
        var getGroupsByPage = new SmartChargingApi.Groups.Features.GetGroupsByPage.v1.GetGroupsByPage(1, pageSize);
        var handler = Scope.ServiceProvider.GetRequiredService<GetGroupsByPageHandler>();
        var result = await handler.Handle(getGroupsByPage, CancellationToken.None);

        // Assert
        result.Groups.ShouldNotBeNull();
        result.Groups.ShouldNotBeEmpty();
        result.PageCount.ShouldBe(expectedPageCount);
        result.PageSize.ShouldBe(pageSize);
        result.TotalCount.ShouldBe(totalGroups);
    }
}
