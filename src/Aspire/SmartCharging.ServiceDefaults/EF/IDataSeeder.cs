using Microsoft.EntityFrameworkCore;

namespace SmartCharging.ServiceDefaults.EF;

public interface IDataSeeder<in TContext>
    where TContext : DbContext
{
    Task SeedAsync(TContext context);
}
