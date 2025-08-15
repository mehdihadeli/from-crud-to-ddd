using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SmartCharging.ServiceDefaults.EF;
using SmartCharging.Shared.Data;
using SmartChargingApi.Shared.Data;

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
