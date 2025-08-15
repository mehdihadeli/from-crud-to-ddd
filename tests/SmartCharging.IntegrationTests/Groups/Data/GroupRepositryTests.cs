using Microsoft.Extensions.DependencyInjection;
using SmartCharging.IntegrationTests.Groups.Mocks;
using SmartCharging.Shared.Data;
using SmartCharging.TestsShared.Fixtures;
using SmartChargingApi;
using SmartChargingApi.Groups.Contracts;
using SmartChargingApi.Groups.Models.ValueObjects;
using SmartChargingApi.Shared.Contracts;
using SmartChargingApi.Shared.Data;

namespace SmartCharging.IntegrationTests.Groups.Data;

public class GroupRepositoryTests : SmartChargingIntegrationTestBase
{
    private readonly IGroupRepository _groupRepository;
    private readonly IUnitOfWork _unitOfWork;

    public GroupRepositoryTests(SharedFixture<SmartChargingMetadata, SmartChargingDbContext> sharedFixture)
        : base(sharedFixture)
    {
        _groupRepository = base.Scope.ServiceProvider.GetRequiredService<IGroupRepository>();
        _unitOfWork = base.Scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
    }

    [Fact]
    internal async Task AddAsync_WhenGroupIsValid_AddsGroupSuccessfully()
    {
        // Arrange
        var fakeGroup = new GroupFake(numberOfConnectorsPerStation: 3).Generate();

        // Act
        await _groupRepository.AddAsync(fakeGroup, CancellationToken.None);
        await _unitOfWork.CommitAsync(TestContext.Current.CancellationToken);

        // Assert
        var exists = await _groupRepository.ExistsAsync(fakeGroup.Id, TestContext.Current.CancellationToken);
        exists.ShouldBeTrue();
    }

    [Fact]
    internal async Task GetByIdAsync_WhenGroupExists_ReturnsGroupWithDetails()
    {
        // Arrange
        var fakeGroup = new GroupFake(numberOfConnectorsPerStation: 3).Generate();

        await SharedFixture.ExecuteEfDbContextAsync(async db =>
        {
            await db.Groups.AddAsync(fakeGroup);
            await db.SaveChangesAsync();
        });

        // Act
        var retrievedGroup = await _groupRepository.GetByIdAsync(fakeGroup.Id, TestContext.Current.CancellationToken);

        // Assert
        retrievedGroup.ShouldNotBeNull();
        retrievedGroup.Id.ShouldBe(fakeGroup.Id);
        retrievedGroup.ChargeStations.Count.ShouldBe(fakeGroup.ChargeStations.Count);
    }

    [Fact]
    internal async Task GetGroupsByPageAsync_WhenPageNumberAndSizeAreValid_ReturnsPagedResults()
    {
        // Arrange
        for (var i = 0; i < 10; i++)
        {
            var fakeGroup = new GroupFake(numberOfConnectorsPerStation: 3).Generate();
            await SharedFixture.ExecuteEfDbContextAsync(async db =>
            {
                await db.Groups.AddAsync(fakeGroup);
                await db.SaveChangesAsync();
            });
        }

        // Act
        var groups = await _groupRepository.GetByPageAsync(1, 5, TestContext.Current.CancellationToken);

        // Assert
        groups.ShouldNotBeNull();
        groups.Count.ShouldBe(5);
    }

    [Fact]
    internal async Task ExistsAsync_WhenGroupExists_ReturnsTrue()
    {
        // Arrange
        var fakeGroup = new GroupFake(numberOfConnectorsPerStation: 3).Generate();

        await SharedFixture.ExecuteEfDbContextAsync(async db =>
        {
            await db.Groups.AddAsync(fakeGroup);
            await db.SaveChangesAsync();
        });

        // Act
        var exists = await _groupRepository.ExistsAsync(fakeGroup.Id, TestContext.Current.CancellationToken);

        // Assert
        exists.ShouldBeTrue();
    }

    [Fact]
    internal async Task ExistsAsync_WhenGroupDoesNotExist_ReturnsFalse()
    {
        // Act
        var nonExistentGroupId = GroupId.New();
        var exists = await _groupRepository.ExistsAsync(nonExistentGroupId, TestContext.Current.CancellationToken);

        // Assert
        exists.ShouldBeFalse();
    }

    [Fact]
    internal async Task Remove_WhenGroupExists_RemovesGroupSuccessfully()
    {
        // Arrange
        var fakeGroup = new GroupFake(numberOfConnectorsPerStation: 3).Generate();

        await SharedFixture.ExecuteEfDbContextAsync(async db =>
        {
            await db.Groups.AddAsync(fakeGroup);
            await db.SaveChangesAsync();
        });

        // Act
        var groupToRemove = await _groupRepository.GetByIdAsync(fakeGroup.Id, TestContext.Current.CancellationToken);
        if (groupToRemove != null)
        {
            // remove applying to a scoped dbcontext that uses also inside the unitofwork
            _groupRepository.Remove(groupToRemove);
            await _unitOfWork.CommitAsync(TestContext.Current.CancellationToken);
        }

        // Assert
        var exists = await _groupRepository.ExistsAsync(fakeGroup.Id, TestContext.Current.CancellationToken);
        exists.ShouldBeFalse();
    }
}
