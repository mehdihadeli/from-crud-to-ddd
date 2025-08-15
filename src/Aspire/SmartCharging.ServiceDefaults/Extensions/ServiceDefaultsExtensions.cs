using System.Reflection;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ServiceDiscovery;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using SmartCharging.ServiceDefaults.Diagnostics;
using SmartCharging.ServiceDefaults.Diagnostics.Extensions;

namespace SmartCharging.ServiceDefaults.Extensions;

// Adds common .NET Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
public static class ServiceDefaultsExtensions
{
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "/alive";
    private const string HealthChecks = nameof(HealthChecks);

    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();

        // https://learn.microsoft.com/en-us/dotnet/core/extensions/service-discovery?tabs=dotnet-cli#example-usage
        builder.Services.AddServiceDiscovery();

        builder.Services.AddHttpContextAccessor();

        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-logging/
        builder.Services.AddHttpLogging(o =>
        {
            o.CombineLogs = true;
            o.LoggingFields = HttpLoggingFields.All | HttpLoggingFields.RequestQuery;
        });

        // https://learn.microsoft.com/en-us/dotnet/core/extensions/service-discovery?tabs=dotnet-cli#scheme-selection-when-resolving-https-endpoints
        builder.Services.Configure<ServiceDiscoveryOptions>(options => options.AllowAllSchemes = true);

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler(options =>
            {
                var timeSpan = TimeSpan.FromMinutes(2);
                options.AttemptTimeout.Timeout = timeSpan;
                options.CircuitBreaker.SamplingDuration = timeSpan * 2;
                options.TotalRequestTimeout.Timeout = timeSpan * 3;
                options.Retry.MaxRetryAttempts = 1;
            });

            // https://learn.microsoft.com/en-us/dotnet/core/extensions/service-discovery?tabs=dotnet-cli#example-usage
            // Turn on service discovery by default on all http clients
            http.AddServiceDiscovery();
        });

        builder.AddDiagnostics(builder.Configuration.GetValue<string>("InstrumentationName") ?? "smart-charging");

        return builder;
    }

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.EnableEnrichment();
        builder.Logging.AddGlobalBuffer(builder.Configuration.GetSection("Logging"));
        builder.Logging.AddPerIncomingRequestBuffer(builder.Configuration.GetSection("Logging"));
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        if (builder.Environment.IsDevelopment())
        {
            builder.Logging.AddTraceBasedSampler();
        }

        builder
            .Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter("EventManagement");
            })
            .WithTracing(tracing =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    tracing.SetSampler(new AlwaysOnSampler());
                }

                tracing
                    .AddSource(builder.Environment.ApplicationName)
                    .AddAspNetCoreInstrumentation(options =>
                        // Don't trace requests to the health endpoint to avoid filling the dashboard with noise
                        options.Filter = httpContext =>
                            !(
                                httpContext.Request.Path.StartsWithSegments(HealthEndpointPath)
                                || httpContext.Request.Path.StartsWithSegments(AlivenessEndpointPath)
                            )
                    )
                    .AddGrpcClientInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddProcessor(new FixHttpRouteProcessor())
                    .AddSource("EventManagement");
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        // Uncomment the following lines to enable the Azure Monitor exporter (requires the Azure.Monitor.OpenTelemetry.AspNetCore package)
        //if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
        //{
        //    builder.Services.AddOpenTelemetry()
        //       .UseAzureMonitor();
        //}

        return builder;
    }

    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/health-checks#non-development-environments
        // ref: https://github.com/dotnet/aspire-samples/blob/main/samples/HealthChecksUI/HealthChecksUI.ServiceDefaults/Extensions.cs#L108
        var services = builder.Services;
        var healthChecksConfiguration = builder.Configuration.GetSection(HealthChecks);

        // All health checks endpoints must return within the configured timeout value (defaults to 5 seconds)
        var healthChecksRequestTimeout =
            healthChecksConfiguration.GetValue<TimeSpan?>("RequestTimeout") ?? TimeSpan.FromSeconds(5);

        services.AddRequestTimeouts(timeouts => timeouts.AddPolicy(HealthChecks, healthChecksRequestTimeout));

        // Cache health checks responses for the configured duration (defaults to 10 seconds)
        var healthChecksExpireAfter =
            healthChecksConfiguration.GetValue<TimeSpan?>("ExpireAfter") ?? TimeSpan.FromSeconds(10);

        services.AddOutputCache(caching =>
            caching.AddPolicy(HealthChecks, policy => policy.Expire(healthChecksExpireAfter))
        );

        services
            .AddHealthChecks()
            // Add a default liveness check to ensure the app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static WebApplication UseDefaultServices(this WebApplication app)
    {
        app.UseExceptionHandler(new ExceptionHandlerOptions { AllowStatusCode404Response = true });
        // Handles non-exceptional status codes (e.g., 404 from Results.NotFound(), 401 from unauthorized access) and returns standardized ProblemDetails responses
        app.UseStatusCodePages();

        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-logging/
        app.UseHttpLogging();

        return app;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Adding health checks endpoints to applications in non-development environments has security implications.
        // See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.

        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/aspnetcore-openapi?view=aspnetcore-9.0&tabs=visual-studio%2Cvisual-studio-code#customizing-run-time-behavior-during-build-time-document-generation
        if (Assembly.GetEntryAssembly()?.GetName().Name == "GetDocument.Insider")
        {
            // don't register services in openapi document build by ApiDescription.Server
            return app;
        }

        // ref: https://github.com/dotnet/aspire-samples/tree/main/samples/HealthChecksUI
        // Configure the health checks
        var healthChecks = app.MapGroup("");

        // Configure health checks endpoints to use the configured request timeouts and cache policies
        healthChecks.CacheOutput(HealthChecks).WithRequestTimeout(HealthChecks);

        // All health checks must pass for the app to be considered ready to accept traffic after starting
        healthChecks.MapHealthChecks(HealthEndpointPath);

        // Only health checks tagged with the "live" tag must pass for the app to be considered alive
        healthChecks.MapHealthChecks(AlivenessEndpointPath, new() { Predicate = r => r.Tags.Contains("live") });

        // Add the health checks endpoint for the HealthChecksUI
        var healthChecksUrls = app.Configuration["HEALTHCHECKSUI_URLS"];
        if (string.IsNullOrWhiteSpace(healthChecksUrls))
        {
            return app;
        }

        var pathToHostsMap = GetPathToHostsMap(healthChecksUrls);

        foreach (var path in pathToHostsMap.Keys)
        {
            // Ensure that the HealthChecksUI endpoint is only accessible from configured hosts,
            // e.g., localhost:12345, hub.docker.internal, etc.
            // as it contains more detailed information about the health of the app,
            // including the types of dependencies it has.
            healthChecks
                .MapHealthChecks(path, new() { ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse })
                // This ensures that the HealthChecksUI endpoint is only accessible from the configured health checks URLs.
                // See this documentation to learn more about restricting access to health checks endpoints via routing:
                // https://learn.microsoft.com/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-8.0#use-health-checks-routing
                .RequireHost(pathToHostsMap[path]);
        }

        if (!app.Environment.IsDevelopment())
        {
            app.MapGet("/", () => Results.Redirect(HealthEndpointPath)).ExcludeFromDescription();
        }

        return app;
    }

    private static Dictionary<string, string[]> GetPathToHostsMap(string healthChecksUrls)
    {
        // Given a value like "localhost:12345/healthz;hub.docker.internal:12345/healthz" return a dictionary like:
        // { { "healthz", [ "localhost:12345", "hub.docker.internal:12345" ] } }
        var uris = healthChecksUrls
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(url => new Uri(url, UriKind.Absolute))
            .GroupBy(uri => uri.AbsolutePath, uri => uri.Authority)
            .ToDictionary(g => g.Key, g => g.ToArray());

        return uris;
    }
}
