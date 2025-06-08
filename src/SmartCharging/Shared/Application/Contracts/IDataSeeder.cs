using Microsoft.EntityFrameworkCore;

namespace SmartCharging.Shared.Application.Contracts;

public interface IDataSeeder<in TContext>
    where TContext : DbContext
{
    Task SeedAsync(TContext context);
}
