using Bogus;
using SmartCharging.Shared.Application.Data;
using SmartCharging.UnitTests.Groups.Mocks;

namespace SmartCharging.UnitTests.TestHelpers;

public static class UnitOfWorkFixture
{
    public static UnitOfWork CreateUnitOfWork()
    {
        var dbContext = InMemoryDbContextFactory.Create();
        return new UnitOfWork(dbContext, new GroupRepository(dbContext));
    }

    public static async Task SeedGroupsAsync(UnitOfWork unitOfWork, int count)
    {
        var groupFake = new GroupFake(
            numberOfConnectorsPerStation: 3,
            groupCapacity: new Faker().PickRandom(new List<int?> { 400, 450, 500, 550 }),
            maxConnectorCurrent: new Faker().PickRandom(new List<int?> { 50, 55, 60, 65 })
        );
        var groups = groupFake.Generate(count);

        foreach (var group in groups)
        {
            await unitOfWork.GroupRepository.AddAsync(group, CancellationToken.None);
        }

        await unitOfWork.CommitAsync(CancellationToken.None);
    }
}
