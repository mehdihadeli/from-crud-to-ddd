using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Xunit.Abstractions;

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

    /// <summary>
    /// Use for tracking occured log events for testing purposes
    /// </summary>
    public void WithTestConfigureServices(Action<IServiceCollection> services)
    {
        _testConfigureServices += services;
    }

    public void WithTestConfiguration(Action<IConfiguration> configurations)
    {
        _testConfiguration += configurations;
    }

    public void WithTestConfigureAppConfiguration(
        Action<WebHostBuilderContext, IConfigurationBuilder> appConfigurations
    )
    {
        _testConfigureAppConfiguration += appConfigurations;
    }

    public void AddTestHostedService<THostedService>()
        where THostedService : class, IHostedService
    {
        _testHostedServicesTypes.Add(typeof(THostedService));
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment("test");
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
            // services.RemoveAll<IHostedService>();
            _testConfigureServices?.Invoke(services);
        });

        base.ConfigureWebHost(builder);
    }

    public void AddOverrideInMemoryConfig(Action<IDictionary<string, string>> inmemoryConfigsAction)
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
    }

    public void AddOverrideEnvKeyValues(Action<IDictionary<string, string>> keyValuesAction)
    {
        var keyValues = new Dictionary<string, string>();
        keyValuesAction.Invoke(keyValues);

        foreach (var (key, value) in keyValues)
        {
            // overriding app configs with using environments
            Environment.SetEnvironmentVariable(key, value);
        }
    }
}
