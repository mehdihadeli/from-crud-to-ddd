using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SmartCharging.Shared.Application.Data;
using SmartCharging.Shared.BuildingBlocks.EF;

namespace SmartCharging.UnitTests.TestHelpers;

public static class InMemoryDbContextFactory
{
    public static SmartChargingDbContext Create()
    {
        var options = new DbContextOptionsBuilder<SmartChargingDbContext>()
            .UseSqlite("DataSource=:memory:")
            .EnableSensitiveDataLogging()
            .ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector<Guid>>()
            .Options;

        var dbContext = new SmartChargingDbContext(options);
        dbContext.Database.OpenConnection();
        dbContext.Database.EnsureCreated();

        return dbContext;
    }
}
