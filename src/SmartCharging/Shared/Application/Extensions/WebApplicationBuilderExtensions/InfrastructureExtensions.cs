using SmartCharging.Shared.BuildingBlocks.OpenApi;
using SmartCharging.Shared.BuildingBlocks.ProblemDetails;
using SmartCharging.Shared.BuildingBlocks.Versioning;

namespace SmartCharging.Shared.Application.Extensions.WebApplicationBuilderExtensions;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication();
        builder.Services.AddAuthorization();

        builder.AddVersioning();
        builder.AddAspnetOpenApi(["v1"]);

        builder.AddCustomProblemDetails();

        return builder;
    }
}
