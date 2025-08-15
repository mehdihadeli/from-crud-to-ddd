using SmartCharging.ServiceDefaults.OpenApi;
using SmartCharging.ServiceDefaults.ProblemDetails;
using SmartCharging.ServiceDefaults.SystemTextSerializer;
using SmartCharging.ServiceDefaults.Versioning;

namespace SmartChargingApi.Shared.Extensions.WebApplicationBuilderExtensions;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
    {
        // Apply to other places rather than controller response like openapi document generation, and customizes the default JSON serialization behavior for Minimal APIs
        builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
        {
            SystemTextJsonSerializerOptions.SetDefaultOptions(options.SerializerOptions);
        });

        builder.Services.AddAuthentication();
        builder.Services.AddAuthorization();

        builder.AddVersioning();
        builder.AddAspnetOpenApi(["v1"]);

        builder.AddCustomProblemDetails();

        return builder;
    }
}
