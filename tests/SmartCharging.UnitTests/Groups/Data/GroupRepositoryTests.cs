using Shouldly;
using SmartCharging.Groups.Data;
using SmartCharging.Groups.Models;
using SmartCharging.Groups.Models.ValueObjects;
using SmartCharging.Shared.Application.Data;
using SmartCharging.Shared.BuildingBlocks.Exceptions;
using SmartCharging.UnitTests.Groups.Mocks;
using SmartCharging.UnitTests.TestHelpers;

namespace SmartCharging.UnitTests.Groups.Data;

public class GroupRepositoryTests
{
    private readonly SmartChargingDbContext _dbContext;
    private readonly GroupRepository _groupRepository;

    public GroupRepositoryTests()
    {
        _dbContext = InMemoryDbContextFactory.Create();
        _groupRepository = new GroupRepository(_dbContext);
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_Returns_Group_When_Id_Exists()
    {
        // Arrange
        var fakeGroup = new GroupFake().Generate();
        _dbContext.Groups.Add(fakeGroup);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _groupRepository.GetByIdAsync(fakeGroup.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(fakeGroup.Id);
        result.Name.ShouldBe(fakeGroup.Name);
        result.CapacityInAmps.ShouldBe(fakeGroup.CapacityInAmps);
    }

    [Fact]
    public async Task GetByIdAsync_Returns_Null_When_Id_Does_Not_Exist()
    {
        // Arrange
        var nonExistentId = GroupId.New();

        // Act
        var result = await _groupRepository.GetByIdAsync(nonExistentId);

        // Assert
        result.ShouldBeNull();
    }

    #endregion

    #region GetGroupsByPageAsync Tests

    [Fact]
    public async Task GetGroupsByPageAsync_Returns_Paginated_Groups_When_Data_Exists()
    {
        // Arrange
        var fakeGroups = new GroupFake().Generate(10);
        _dbContext.Groups.AddRange(fakeGroups);
        await _dbContext.SaveChangesAsync();

        const int pageNumber = 1;
        const int pageSize = 5;

        // Act
        var result = await _groupRepository.GetByPageAsync(pageNumber, pageSize);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(pageSize);
    }

    [Fact]
    public async Task GetGroupsByPageAsync_Returns_Empty_List_When_Page_Is_Empty()
    {
        // Arrange
        var fakeGroups = new GroupFake().Generate(3);
        _dbContext.Groups.AddRange(fakeGroups);
        await _dbContext.SaveChangesAsync();

        const int pageNumber = 2;
        const int pageSize = 5;

        // Act
        var result = await _groupRepository.GetByPageAsync(pageNumber, pageSize);

        // Assert
        result.ShouldBeEmpty();
    }

    #endregion

    #region ExistsAsync Tests

    [Fact]
    public async Task ExistsAsync_Returns_True_When_Group_Exists()
    {
        // Arrange
        var fakeGroup = new GroupFake().Generate();
        _dbContext.Groups.Add(fakeGroup);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _groupRepository.ExistsAsync(fakeGroup.Id);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task ExistsAsync_Returns_False_When_Group_Does_Not_Exist()
    {
        // Arrange
        var nonExistentId = GroupId.New();

        // Act
        var result = await _groupRepository.ExistsAsync(nonExistentId);

        // Assert
        result.ShouldBeFalse();
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_Adds_Group_To_Database()
    {
        // Arrange
        var fakeGroup = new GroupFake().Generate();

        // Act
        await _groupRepository.AddAsync(fakeGroup);
        await _dbContext.SaveChangesAsync();

        // Assert
        _dbContext.Groups.ShouldContain(fakeGroup);
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_Updates_Group_In_Database()
    {
        // Arrange
        var fakeGroup = new GroupFake().Generate();
        _dbContext.Groups.Add(fakeGroup);
        _dbContext.SaveChanges();

        var updatedCapacity = CurrentInAmps.Of(888);

        // Act
        fakeGroup.UpdateCapacity(updatedCapacity);
        _groupRepository.Update(fakeGroup);
        _dbContext.SaveChanges();

        // Assert
        var updatedGroup = _dbContext.Groups.Find(fakeGroup.Id);
        updatedGroup.ShouldNotBeNull();
        updatedGroup.CapacityInAmps.ShouldBe(updatedCapacity);
    }

    [Fact]
    public void Update_Throws_Exception_When_Group_Is_Null()
    {
        // Arrange
        Group? nullGroup = null;

        // Act & Assert
        Should.Throw<ValidationException>(() => _groupRepository.Update(nullGroup!));
    }

    #endregion

    #region Remove Tests

    [Fact]
    public void Remove_Removes_Group_From_Database()
    {
        // Arrange
        var fakeGroup = new GroupFake().Generate();
        _dbContext.Groups.Add(fakeGroup);
        _dbContext.SaveChanges();

        // Act
        _groupRepository.Remove(fakeGroup);
        _dbContext.SaveChanges();

        // Assert
        _dbContext.Groups.ShouldNotContain(fakeGroup);
    }

    [Fact]
    public void Remove_Does_Nothing_When_Group_Is_Null()
    {
        // Arrange
        Group? nullGroup = null;

        // Act & Assert
        Should.Throw<ValidationException>(() => _groupRepository.Remove(nullGroup!));
    }

    #endregion
}
