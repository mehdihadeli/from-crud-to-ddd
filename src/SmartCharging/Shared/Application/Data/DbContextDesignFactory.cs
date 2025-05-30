using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SmartCharging.Shared.BuildingBlocks.EF;

namespace SmartCharging.Shared.Application.Data;

public class CatalogDbContextDesignFactory : IDesignTimeDbContextFactory<SmartChargingDbContext>
{
    public SmartChargingDbContext CreateDbContext(string[] args)
    {
        Console.WriteLine($"BaseDirectory: {AppContext.BaseDirectory}");

        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environments.Development;

        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory ?? "")
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{environmentName}.json", true) // it is optional
            .AddEnvironmentVariables();

        var configuration = builder.Build();

        Console.WriteLine(Constants.Database.SmartCharging);
        Console.WriteLine(configuration.GetConnectionString(Constants.Database.SmartCharging));

        var optionsBuilder = new DbContextOptionsBuilder<SmartChargingDbContext>()
            .UseNpgsql(
                configuration.GetConnectionString(Constants.Database.SmartCharging),
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(GetType().Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
                }
            )
            .UseSnakeCaseNamingConvention();

        optionsBuilder.ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector<Guid>>();

        return (SmartChargingDbContext)Activator.CreateInstance(typeof(SmartChargingDbContext), optionsBuilder.Options);
    }
}
