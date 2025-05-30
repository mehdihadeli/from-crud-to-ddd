using Microsoft.Extensions.DependencyInjection;
using SmartCharging.Groups.Features.GroupGetById.v1;
using SmartCharging.Groups.Models.ValueObjects;
using SmartCharging.IntegrationTests.Groups.Mocks;
using SmartCharging.Shared.Application.Data;
using SmartCharging.Shared.BuildingBlocks.Exceptions;
using SmartCharging.TestsShared.Fixtures;
using Xunit.Abstractions;

namespace SmartCharging.IntegrationTests.Groups.Features.GroupGetById.v1;

public class GroupGetByIdTests(
    SharedFixture<SmartChargingMetadata, SmartChargingDbContext> sharedFixture,
    ITestOutputHelper outputHelper
) : SmartChargingIntegrationTestBase(sharedFixture, outputHelper)
{
    [Fact]
    internal async Task GroupGetById_WithValidGroupId_Should_ReturnGroupDetails()
    {
        // Arrange
        var fakeGroup = new GroupFake().Generate(1).First();
        await SharedFixture.InsertEfDbContextAsync(fakeGroup);

        var groupGetById = new SmartCharging.Groups.Features.GroupGetById.v1.GroupGetById(
            GroupId.Of(fakeGroup.Id.Value)
        );
        var handler = Scope.ServiceProvider.GetRequiredService<GroupGetByIdHandler>();

        // Act
        var result = await handler.Handle(groupGetById, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Group.ShouldNotBeNull();
        result.Group.GroupId.ShouldBe(fakeGroup.Id.Value);
        result.Group.Name.ShouldBe(fakeGroup.Name.Value);
        result.Group.CapacityInAmps.ShouldBe(fakeGroup.CapacityInAmps.Value);
    }

    [Fact]
    internal async Task GroupGetById_WithInvalidGroupId_Should_ThrowNotFoundException()
    {
        // Arrange
        var invalidGroupId = GroupId.Of(Guid.NewGuid());
        var groupGetById = new SmartCharging.Groups.Features.GroupGetById.v1.GroupGetById(invalidGroupId);
        var handler = Scope.ServiceProvider.GetRequiredService<GroupGetByIdHandler>();

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(() => handler.Handle(groupGetById, CancellationToken.None));
    }

    [Fact]
    internal async Task GroupGetById_WithNullInput_Should_ThrowValidationException()
    {
        // Arrange
        var handler = Scope.ServiceProvider.GetRequiredService<GroupGetByIdHandler>();

        // Act & Assert
        await Should.ThrowAsync<ValidationException>(() => handler.Handle(null!, CancellationToken.None));
    }
}
