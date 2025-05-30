using Microsoft.EntityFrameworkCore;
using SmartCharging.Groups.Models;

namespace SmartCharging.Shared.Application.Data;

public class SmartChargingDbContext(DbContextOptions<SmartChargingDbContext> options) : DbContext(options)
{
    public DbSet<Group> Groups { get; set; }
    public DbSet<ChargeStation> ChargeStations { get; set; }
    public DbSet<Connector> Connectors { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SmartChargingDbContext).Assembly);
    }
}
