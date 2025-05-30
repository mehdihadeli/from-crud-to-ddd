using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartCharging.TestsShared.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace SmartCharging.TestsShared.TestBases;

public abstract class IntegrationTestBase<TEntryPoint, TContext> : IAsyncLifetime
    where TEntryPoint : class
    where TContext : DbContext
{
    private IServiceScope? _serviceScope;

    protected CancellationToken CancellationToken => CancellationTokenSource.Token;
    protected CancellationTokenSource CancellationTokenSource { get; }
    protected int Timeout => 180;

    // Build Service Provider here
    protected IServiceScope Scope => _serviceScope ??= SharedFixture.ServiceProvider.CreateScope();
    protected SharedFixture<TEntryPoint, TContext> SharedFixture { get; }

    protected IntegrationTestBase(SharedFixture<TEntryPoint, TContext> sharedFixture, ITestOutputHelper outputHelper)
    {
        SharedFixture = sharedFixture;
        SharedFixture.SetOutputHelper(outputHelper);

        CancellationTokenSource = new(TimeSpan.FromSeconds(Timeout));
        CancellationToken.ThrowIfCancellationRequested();

        // we should not build a factory service provider with getting ServiceProvider in SharedFixture construction to having capability for override
        SharedFixture.WithTestConfigureServices(SetupTestConfigureServices);
        SharedFixture.WithTestConfigureAppConfiguration(
            (context, configurationBuilder) =>
            {
                SetupTestConfigureAppConfiguration(context, context.Configuration, context.HostingEnvironment);
            }
        );
        SharedFixture.WithTestConfiguration(SetupTestConfiguration);
        SharedFixture.AddOverrideEnvKeyValues(OverrideEnvKeyValues);
        SharedFixture.AddOverrideInMemoryConfig(OverrideInMemoryConfig);

        // Note: building service provider here
    }

    // we use IAsyncLifetime in xunit instead of constructor when we have an async operation
    public virtual async Task InitializeAsync() { }

    public virtual async Task DisposeAsync()
    {
        // it is better messages delete it first
        await SharedFixture.ResetDatabasesAsync();

        await CancellationTokenSource.CancelAsync();

        Scope.Dispose();
    }

    protected virtual void SetupTestConfigureServices(IServiceCollection services) { }

    protected virtual void SetupTestConfigureAppConfiguration(
        WebHostBuilderContext webHostBuilderContext,
        IConfiguration configuration,
        IWebHostEnvironment hostingEnvironment
    ) { }

    protected virtual void SetupTestConfiguration(IConfiguration configurations) { }

    protected virtual void OverrideEnvKeyValues(IDictionary<string, string> keyValues) { }

    protected virtual void OverrideInMemoryConfig(IDictionary<string, string> keyValues) { }
}
