using Asp.Versioning;

namespace SmartCharging.Shared.BuildingBlocks.Versioning;

public static class Extensions
{
    public static void AddVersioning(this WebApplicationBuilder builder)
    {
        builder
            .Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new(1, 0);
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'V";
                options.SubstituteApiVersionInUrl = true;
            });
    }
}
