using Bogus;
using NSubstitute;
using SmartCharging.UnitTests.Groups.Mocks;
using SmartChargingApi.Groups.Features.GetGroupsByPage.v1;
using SmartChargingApi.Groups.Models;
using SmartChargingApi.Shared.Contracts;

namespace SmartCharging.UnitTests.Groups.Features.GetGroupByPage.v1;

public class GetGroupsByPageHandlerTests
{
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly GetGroupsByPageHandler _handler;

    public GetGroupsByPageHandlerTests()
    {
        // Mock the UnitOfWork and its GroupRepository
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _handler = new GetGroupsByPageHandler(_unitOfWorkMock);
    }

    [Fact]
    public async Task Handle_WithValidData_ReturnsPaginatedGroups()
    {
        // Arrange
        // Create fake group data
        var groupFake = new GroupFake(
            numberOfConnectorsPerStation: 3,
            groupCapacity: new Faker().PickRandom(new List<int?> { 400, 450, 500, 550 }),
            maxConnectorCurrent: new Faker().PickRandom(new List<int?> { 50, 55, 60, 65 })
        );
        var groups = groupFake.Generate(10);

        const int pageNumber = 1;
        const int pageSize = 5;
        _unitOfWorkMock
            .GroupRepository.GetByPageAndTotalCountAsync(pageNumber, pageSize, Arg.Any<CancellationToken>())
            .Returns((groups.Take(pageSize).ToList(), groups.Count));

        var getGroupsByPage = GetGroupsByPage.Of(pageNumber, pageSize);

        // Act
        var result = await _handler.Handle(getGroupsByPage, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Groups.ShouldNotBeEmpty(); // Should return groups
        result.Groups.Count.ShouldBe(pageSize); // Should match page size
        result.PageSize.ShouldBe(pageSize);
        result.TotalCount.ShouldBe(groups.Count);
        result.PageCount.ShouldBe((int)Math.Ceiling(groups.Count / (double)pageSize));
        result.Groups.First().Name.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Handle_WithEmptyData_ReturnsEmptyList()
    {
        // Arrange
        const int pageNumber = 2;
        const int pageSize = 5;

        _unitOfWorkMock
            .GroupRepository.GetByPageAndTotalCountAsync(pageNumber, pageSize, Arg.Any<CancellationToken>())
            .Returns((new List<Group>(), 0));

        var getGroupsByPage = GetGroupsByPage.Of(pageNumber, pageSize);

        // Act
        var result = await _handler.Handle(getGroupsByPage, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Groups.ShouldNotBeNull(); // Ensure groups is not null
        result.Groups.ShouldBeEmpty(); // Ensure the groups list is empty
        result.TotalCount.ShouldBe(0);
        result.PageCount.ShouldBe(0);
    }

    [Fact]
    public void Handle_WithNullInput_ThrowsValidationException()
    {
        // Act & Assert
        Should.ThrowAsync<ValidationException>(() => _handler.Handle(null!, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_PassesCorrectParametersToRepository()
    {
        // Arrange
        const int pageNumber = 3;
        const int pageSize = 10;
        var getGroupsByPage = GetGroupsByPage.Of(pageNumber, pageSize);

        _unitOfWorkMock
            .GroupRepository.GetByPageAndTotalCountAsync(
                Arg.Is(pageNumber),
                Arg.Is(pageSize),
                Arg.Any<CancellationToken>()
            )
            .Returns((new List<Group>(), 0));

        // Act
        await _handler.Handle(getGroupsByPage, CancellationToken.None);

        // Assert
        await _unitOfWorkMock
            .GroupRepository.Received(1)
            .GetByPageAndTotalCountAsync(Arg.Is(pageNumber), Arg.Is(pageSize), Arg.Any<CancellationToken>());
    }
}
