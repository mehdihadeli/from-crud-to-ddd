using Shouldly;
using SmartCharging.Groups.Features.GetGroupsByPage.v1;
using SmartCharging.Shared.BuildingBlocks.Exceptions;

namespace SmartCharging.UnitTests.Groups.Features.GetGroupByPage.v1;

public class GetGroupsByPageTests
{
    [Fact]
    public void Of_WithValidParameters_CreatesInstanceSuccessfully()
    {
        // Arrange
        const int pageNumber = 2;
        const int pageSize = 10;

        // Act
        var result = GetGroupsByPage.Of(pageNumber, pageSize);

        // Assert
        result.ShouldNotBeNull();
        result.PageNumber.ShouldBe(pageNumber);
        result.PageSize.ShouldBe(pageSize);
    }

    [Fact]
    public void Of_WithNegativePageNumber_ThrowsValidationException()
    {
        // Arrange
        const int invalidPageNumber = -1;
        const int pageSize = 5;

        // Act & Assert
        Should
            .Throw<ValidationException>(() => GetGroupsByPage.Of(invalidPageNumber, pageSize))
            .Message.ShouldContain("pageNumber cannot be negative or zero.");
    }

    [Fact]
    public void Of_WithZeroPageNumber_ThrowsValidationException()
    {
        // Arrange
        const int invalidPageNumber = 0;
        const int pageSize = 5;

        // Act & Assert
        Should
            .Throw<ValidationException>(() => GetGroupsByPage.Of(invalidPageNumber, pageSize))
            .Message.ShouldContain("pageNumber cannot be negative or zero.");
    }

    [Fact]
    public void Of_WithZeroPageSize_ThrowsValidationException()
    {
        // Arrange
        const int pageNumber = 1;
        const int invalidPageSize = 0;

        // Act & Assert
        Should
            .Throw<ValidationException>(() => GetGroupsByPage.Of(pageNumber, invalidPageSize))
            .Message.ShouldContain("pageSize cannot be negative or zero.");
    }

    [Fact]
    public void Of_WithNegativePageSize_ThrowsValidationException()
    {
        // Arrange
        const int pageNumber = 1;
        const int invalidPageSize = -5;

        // Act & Assert
        Should
            .Throw<ValidationException>(() => GetGroupsByPage.Of(pageNumber, invalidPageSize))
            .Message.ShouldContain("pageSize cannot be negative or zero.");
    }
}
