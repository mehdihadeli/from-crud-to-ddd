using SmartCharging.Shared.BuildingBlocks.EF;

namespace SmartCharging.Shared.Application.Data;

public class SmartChargingDataSeeder : IDataSeeder<SmartChargingDbContext>
{
    public Task SeedAsync(SmartChargingDbContext context)
    {
        // seeding data for dev environment
        return Task.CompletedTask;
    }
}
