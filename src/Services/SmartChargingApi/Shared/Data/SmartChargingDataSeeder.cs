namespace SmartChargingApi.Shared.Data;

public class SmartChargingDataSeeder : SmartCharging.ServiceDefaults.EF.IDataSeeder<SmartChargingDbContext>
{
    public Task SeedAsync(SmartChargingDbContext context)
    {
        // seeding data for dev environment
        return Task.CompletedTask;
    }
}
