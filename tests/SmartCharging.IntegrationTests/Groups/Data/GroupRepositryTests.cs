using Microsoft.Extensions.DependencyInjection;
using SmartCharging.Groups.Contracts;
using SmartCharging.Groups.Models.ValueObjects;
using SmartCharging.IntegrationTests.Groups.Mocks;
using SmartCharging.Shared.Application.Data;
using SmartCharging.TestsShared.Fixtures;
using Xunit.Abstractions;

namespace SmartCharging.IntegrationTests.Groups.Data;

public class GroupRepositoryTests : SmartChargingIntegrationTestBase
{
    private readonly IGroupRepository _groupRepository;
    private readonly IUnitOfWork _unitOfWork;

    public GroupRepositoryTests(
        SharedFixture<SmartChargingMetadata, SmartChargingDbContext> sharedFixture,
        ITestOutputHelper outputHelper
    )
        : base(sharedFixture, outputHelper)
    {
        _groupRepository = base.Scope.ServiceProvider.GetRequiredService<IGroupRepository>();
        _unitOfWork = base.Scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
    }

    [Fact]
    internal async Task AddAsync_WithValidGroup_Should_AddGroupSuccessfully()
    {
        // Arrange
        var fakeGroup = new GroupFake(numberOfConnectorsPerStation: 3).Generate();

        // Act
        await _groupRepository.AddAsync(fakeGroup, CancellationToken.None);
        await _unitOfWork.CommitAsync();

        // Assert
        var exists = await _groupRepository.ExistsAsync(fakeGroup.Id);
        exists.ShouldBeTrue();
    }

    [Fact]
    internal async Task GetByIdAsync_WithExistingGroup_Should_ReturnGroupWithDetails()
    {
        // Arrange
        var fakeGroup = new GroupFake(numberOfConnectorsPerStation: 3).Generate();

        await SharedFixture.ExecuteEfDbContextAsync(async db =>
        {
            await db.Groups.AddAsync(fakeGroup);
            await db.SaveChangesAsync();
        });

        // Act
        var retrievedGroup = await _groupRepository.GetByIdAsync(fakeGroup.Id);

        // Assert
        retrievedGroup.ShouldNotBeNull();
        retrievedGroup.Id.ShouldBe(fakeGroup.Id);
        retrievedGroup.ChargeStations.Count.ShouldBe(fakeGroup.ChargeStations.Count);
    }

    [Fact]
    internal async Task GetGroupsByPageAsync_WithValidPageNumberAndSize_Should_ReturnPagedResults()
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
        var groups = await _groupRepository.GetGroupsByPageAsync(1, 5);

        // Assert
        groups.ShouldNotBeNull();
        groups.Count.ShouldBe(5);
    }

    [Fact]
    internal async Task ExistsAsync_WithExistingGroup_Should_ReturnTrue()
    {
        // Arrange
        var fakeGroup = new GroupFake(numberOfConnectorsPerStation: 3).Generate();

        await SharedFixture.ExecuteEfDbContextAsync(async db =>
        {
            await db.Groups.AddAsync(fakeGroup);
            await db.SaveChangesAsync();
        });

        // Act
        var exists = await _groupRepository.ExistsAsync(fakeGroup.Id);

        // Assert
        exists.ShouldBeTrue();
    }

    [Fact]
    internal async Task ExistsAsync_WithNonExistentGroup_Should_ReturnFalse()
    {
        // Act
        var nonExistentGroupId = GroupId.New();
        var exists = await _groupRepository.ExistsAsync(nonExistentGroupId);

        // Assert
        exists.ShouldBeFalse();
    }

    [Fact]
    internal async Task Remove_WithExistingGroup_Should_RemoveGroupSuccessfully()
    {
        // Arrange
        var fakeGroup = new GroupFake(numberOfConnectorsPerStation: 3).Generate();

        await SharedFixture.ExecuteEfDbContextAsync(async db =>
        {
            await db.Groups.AddAsync(fakeGroup);
            await db.SaveChangesAsync();
        });

        // Act
        var groupToRemove = await _groupRepository.GetByIdAsync(fakeGroup.Id);
        if (groupToRemove != null)
        {
            // remove applies to a scoped dbcontext that uses also inside unitofwork
            _groupRepository.Remove(groupToRemove);
            await _unitOfWork.CommitAsync();
        }

        // Assert
        var exists = await _groupRepository.ExistsAsync(fakeGroup.Id);
        exists.ShouldBeFalse();
    }
}
