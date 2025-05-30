using System.Reflection;

namespace SmartCharging.Shared.BuildingBlocks.Repository;

public static class DependencyInjectionExtensions
{
    public static void AddRepositories(this WebApplicationBuilder builder, Assembly assembly)
    {
        builder.Services.Scan(scan =>
            scan.FromAssemblies(assembly)
                .AddClasses(classes => classes.AssignableTo(typeof(IRepository<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );
    }
}
