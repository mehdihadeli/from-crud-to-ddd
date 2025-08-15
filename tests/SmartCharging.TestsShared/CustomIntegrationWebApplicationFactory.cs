using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SmartCharging.TestsShared;

public class CustomWebApplicationFactory<TRootMetadata>(Action<IWebHostBuilder>? webHostBuilder = null)
    : WebApplicationFactory<TRootMetadata>
    where TRootMetadata : class
{
    private ITestOutputHelper? _outputHelper;
    private readonly Dictionary<string, string?> _inMemoryConfigs = new();
    private Action<IServiceCollection>? _testConfigureServices;
    private Action<IConfiguration>? _testConfiguration;
    private Action<WebHostBuilderContext, IConfigurationBuilder>? _testConfigureAppConfiguration;
    private readonly List<Type> _testHostedServicesTypes = new();
    private readonly List<string> _overrideEnvKeysToDispose = [];
    private string _environment = "test";

    public CustomWebApplicationFactory<TRootMetadata> WithTestConfigureServices(Action<IServiceCollection> services)
    {
        _testConfigureServices += services;
        return this;
    }

    public CustomWebApplicationFactory<TRootMetadata> WithTestConfiguration(Action<IConfiguration> configurations)
    {
        _testConfiguration += configurations;
        return this;
    }

    public CustomWebApplicationFactory<TRootMetadata> WithTestConfigureAppConfiguration(
        Action<WebHostBuilderContext, IConfigurationBuilder> appConfigurations
    )
    {
        _testConfigureAppConfiguration += appConfigurations;
        return this;
    }

    public CustomWebApplicationFactory<TRootMetadata> WithEnvironment(string environment)
    {
        _environment = environment;
        return this;
    }

    public void AddTestHostedService<THostedService>()
        where THostedService : class, IHostedService
    {
        _testHostedServicesTypes.Add(typeof(THostedService));
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment(_environment);
        builder.UseContentRoot(".");

        builder.UseDefaultServiceProvider(
            (env, c) =>
            {
                // Handling Captive Dependency Problem
                if (env.HostingEnvironment.IsDevelopment())
                    c.ValidateScopes = true;
            }
        );

        return base.CreateHost(builder);
    }

    public void SetOutputHelper(ITestOutputHelper value) => _outputHelper = value;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        webHostBuilder?.Invoke(builder);

        builder.ConfigureAppConfiguration(
            (hostingContext, configurationBuilder) =>
            {
                //// add in-memory configuration instead of using appestings.json and override existing settings and it is accessible via IOptions and Configuration
                //// https://blog.markvincze.com/overriding-configuration-in-asp-net-core-integration-tests/
                configurationBuilder.AddInMemoryCollection(_inMemoryConfigs);

                _testConfiguration?.Invoke(hostingContext.Configuration);
                _testConfigureAppConfiguration?.Invoke(hostingContext, configurationBuilder);
            }
        );

        builder.ConfigureTestServices(services =>
        {
            // https://andrewlock.net/converting-integration-tests-to-net-core-3/
            // add test-hosted services
            foreach (var hostedServiceType in _testHostedServicesTypes)
            {
                services.AddSingleton(typeof(IHostedService), hostedServiceType);
            }

            _testConfigureServices?.Invoke(services);
        });

        base.ConfigureWebHost(builder);
    }

    public CustomWebApplicationFactory<TRootMetadata> AddOverrideInMemoryConfig(
        Action<IDictionary<string, string>> inmemoryConfigsAction
    )
    {
        var inmemoryConfigs = new Dictionary<string, string>();
        inmemoryConfigsAction.Invoke(inmemoryConfigs);

        // overriding app configs with using in-memory configs
        // add in-memory configuration instead of using appestings.json and override existing settings and it is accessible via IOptions and Configuration
        // https://blog.markvincze.com/overriding-configuration-in-asp-net-core-integration-tests/
        foreach (var inmemoryConfig in inmemoryConfigs)
        {
            // Use `TryAdd` for prevent adding repetitive elements because of using IntegrationTestBase
            _inMemoryConfigs.TryAdd(inmemoryConfig.Key, inmemoryConfig.Value);
        }

        return this;
    }

    public CustomWebApplicationFactory<TRootMetadata> AddOverrideEnvKeyValues(
        Action<IDictionary<string, string>> keyValuesAction
    )
    {
        var keyValues = new Dictionary<string, string>();
        keyValuesAction.Invoke(keyValues);

        foreach (var (key, value) in keyValues)
        {
            _overrideEnvKeysToDispose.Add(key);
            // overriding app configs with using environments
            Environment.SetEnvironmentVariable(key, value);
        }

        return this;
    }

    public ValueTask InitializeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public new async Task DisposeAsync()
    {
        CleanupOverrideEnvKeys();

        await base.DisposeAsync();
    }

    private void CleanupOverrideEnvKeys()
    {
        foreach (string disposeEnvKey in _overrideEnvKeysToDispose)
        {
            Environment.SetEnvironmentVariable(disposeEnvKey, null);
        }
    }
}
