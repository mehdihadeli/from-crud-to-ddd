using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SmartCharging.ServiceDefaults.EF;

namespace SmartChargingApi.Shared.Data;

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

        var dbName = "smart_charging";
        Console.WriteLine(dbName);
        Console.WriteLine(configuration.GetConnectionString(dbName));

        var optionsBuilder = new DbContextOptionsBuilder<SmartChargingDbContext>()
            .UseNpgsql(
                configuration.GetConnectionString(dbName),
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
