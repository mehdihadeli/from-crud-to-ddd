using Microsoft.Extensions.DependencyInjection;
using SmartCharging.Groups.Features.GetGroupsByPage.v1;
using SmartCharging.Groups.Models.ValueObjects;
using SmartCharging.IntegrationTests.Groups.Mocks;
using SmartCharging.Shared.Application.Data;
using SmartCharging.TestsShared.Fixtures;
using Xunit.Abstractions;

namespace SmartCharging.IntegrationTests.Groups.Features.GetGroupsByPage.v1;

public class GetGroupsByPageTests(
    SharedFixture<SmartChargingMetadata, SmartChargingDbContext> sharedFixture,
    ITestOutputHelper outputHelper
) : SmartChargingIntegrationTestBase(sharedFixture, outputHelper)
{
    [Fact]
    internal async Task GetGroupsByPage_WithValidData_Should_ReturnExistingGroups()
    {
        // Arrange
        var fakeGroups = new GroupFake().Generate(10);
        await SharedFixture.InsertEfDbContextAsync(fakeGroups.ToArray());

        // Act
        // Request page 1 with 5 groups per page
        var getGroupsByPage = new SmartCharging.Groups.Features.GetGroupsByPage.v1.GetGroupsByPage(1, 5);
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
    internal async Task GetGroupsByPage_WithExistingGroups_Should_ReturnCorrectlyMappedGroups()
    {
        // Arrange
        var fakeGroups = new GroupFake().Generate(10).ToList();

        await SharedFixture.InsertEfDbContextAsync(fakeGroups.ToArray());

        var getGroupsByPage = new SmartCharging.Groups.Features.GetGroupsByPage.v1.GetGroupsByPage(1, 5);
        var handler = Scope.ServiceProvider.GetRequiredService<GetGroupsByPageHandler>();

        // Act
        var result = await handler.Handle(getGroupsByPage, CancellationToken.None);

        // Assert
        result.Groups.ShouldNotBeNull();
        result.Groups.ShouldNotBeEmpty();
        result.Groups.Count.ShouldBe(5);

        // Validate that the returned collection contains the expected data.
        var expectedGroups = fakeGroups.OrderBy(g => g.Name.Value).Take(5);
        foreach (var (returnedGroup, expectedGroup) in result.Groups.Zip(expectedGroups))
        {
            returnedGroup.GroupId.ShouldBe(expectedGroup.Id.Value);
            returnedGroup.Name.ShouldBe(expectedGroup.Name.Value);
            returnedGroup.CapacityInAmps.ShouldBe(expectedGroup.CapacityInAmps.Value);
        }
    }

    [Fact]
    internal async Task GetGroupsByPage_WithEmptyDatabase_Should_ReturnEmptyPagedResult()
    {
        // Act
        var getGroupsByPage = new SmartCharging.Groups.Features.GetGroupsByPage.v1.GetGroupsByPage(1, 5); // Test on an empty database.
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
    internal async Task GetGroupsByPage_WithCustomPageSize_Should_ReturnCorrectPagination(
        int totalGroups,
        int pageSize,
        int expectedPageCount
    )
    {
        // Arrange
        var fakeGroups = new GroupFake().Generate(totalGroups);
        await SharedFixture.InsertEfDbContextAsync(fakeGroups.ToArray());

        // Act
        var getGroupsByPage = new SmartCharging.Groups.Features.GetGroupsByPage.v1.GetGroupsByPage(1, pageSize);
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
