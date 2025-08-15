using EventsManagement.AppHost.Extensions;
using EventsManagement.AppHost.Integrations.HealthChecksUI;
using Humanizer;
using Scalar.Aspire;
using SmartCharging.AppHost.AspireIntegrations;
using SmartCharging.ServiceDefaults.Constants;

var builder = DistributedApplication.CreateBuilder(args);

var appHostLaunchProfile = builder.GetLaunchProfileName();
Console.WriteLine($"AppHost LaunchProfile is: {appHostLaunchProfile}");

var pgUser = builder.AddParameter("pg-user", value: "postgres", publishValueAsDefault: true);
var pgPassword = builder.AddParameter(name: "pg-password", value: new GenerateParameterDefault { MinLength = 3 }, true);

var postgres = builder.AddAspirePostgres(
    AspireResources.Postgres,
    userName: pgUser,
    password: pgPassword,
    initScriptPath: "./../../../deployments/configs/init-postgres.sql"
);
var smartChargingPostgres = postgres.AddAspirePostgresDatabase(
    nameOrConnectionStringName: AspireApplicationResources.PostgresDatabase.SmartCharging,
    databaseName: nameof(AspireApplicationResources.PostgresDatabase.SmartCharging).ToLowerInvariant()
);

var smartChargingStatisticsApi = builder
    .AddProject<Projects.SmartChargingStatisticsApi>(AspireApplicationResources.Api.SmartChargingStatisticsApi)
    .WithReplicas(builder.ExecutionContext.IsRunMode ? 1 : 2)
    .WithFriendlyApiUrls()
    .WithProjectSwaggerUIUrl()
    // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/launch-profiles#control-launch-profile-selection
    // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/networking-overview#launch-profiles
    // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/networking-overview#ports-and-proxies
    // .NET Aspire will parse the launchSettings.json file selecting the appropriate launch profile and automatically generate endpoints
    .WithEndpoint(
        "https",
        endpoint =>
        {
            // - Non-container resources like a project cannot be proxied when both TargetPort and Port are specified with the same value, usually we use Port.
            // - When IsProxy is `true`, proxy uses our launch profile ports, and our app uses a random port, and when IsProxy is `false` the port will use for project prot
            endpoint.IsProxied = true;
        }
    )
    .WithEndpoint(
        "http",
        endpoint =>
        {
            // - Non-container resources like a project cannot be proxied when both TargetPort and Port are specified with the same value, usually we use Port.
            // - When IsProxy is `true`, proxy uses our launch profile ports, and our app uses a random port, and when IsProxy is `false` the port will use for project prot
            endpoint.IsProxied = true;
        }
    );

var smartChargingApi = builder
    .AddProject<Projects.SmartChargingApi>(AspireApplicationResources.Api.SmartChargingApi)
    .WithReplicas(builder.ExecutionContext.IsRunMode ? 1 : 2)
    .WithReference(smartChargingPostgres)
    .WaitFor(smartChargingPostgres)
    .WithReference(smartChargingStatisticsApi)
    .WithFriendlyApiUrls()
    .WithProjectSwaggerUIUrl()
    // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/launch-profiles#control-launch-profile-selection
    // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/networking-overview#launch-profiles
    // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/networking-overview#ports-and-proxies
    // .NET Aspire will parse the launchSettings.json file selecting the appropriate launch profile and automatically generate endpoints
    .WithEndpoint(
        "https",
        endpoint =>
        {
            // - Non-container resources like a project cannot be proxied when both TargetPort and Port are specified with the same value, usually we use Port.
            // - When IsProxy is `true`, proxy uses our launch profile ports, and our app uses a random port, and when IsProxy is `false` the port will use for project prot
            endpoint.IsProxied = true;
        }
    )
    .WithEndpoint(
        "http",
        endpoint =>
        {
            // - Non-container resources like a project cannot be proxied when both TargetPort and Port are specified with the same value, usually we use Port.
            // - When IsProxy is `true`, proxy uses our launch profile ports, and our app uses a random port, and when IsProxy is `false` the port will use for project prot
            endpoint.IsProxied = true;
        }
    );

// https://github.com/dotnet/aspire-samples/tree/main/samples/HealthChecksUI
// The actual service endpoints are not exposed in non-development environments by default because of security implications, we expose them through health check ui with authentication
builder
    .AddHealthChecksUI("healthchecksui".Kebaberize())
    // This will make the HealthChecksUI dashboard available from external networks when deployed.
    // In a production environment, you should consider adding authentication to the ingress layer
    // to restrict access to the dashboard.
    .WithExternalHttpEndpoints()
    .WithReference(smartChargingApi);

if (builder.ExecutionContext.IsRunMode)
{
    // https://github.com/scalar/scalar/blob/fbef7e1ee82d7c9e84bc42407e309642dcec5552/documentation/integrations/aspire.md
    // https://github.com/scalar/scalar/tree/fbef7e1ee82d7c9e84bc42407e309642dcec5552/integrations/aspire
    var scalar = builder
        .AddScalarApiReference(options =>
        {
            options
                .WithTheme(ScalarTheme.BluePlanet)
                .WithTestRequestButton()
                .WithSidebar()
                .WithDefaultFonts(false)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
        })
        .WithApiReference(smartChargingApi, options => options.AddDocument("v1", "events-management-api-v1"));
}

// https//learn.microsoft.com/en-us/dotnet/aspire/whats-new/dotnet-aspire-9.3#deployment--publish
var dockerCompose = builder.AddDockerComposeEnvironment("aspire-docker-compose");

// var kubernetes = builder.AddKubernetesEnvironment("aspire-kubernetes");

builder.Build().Run();
