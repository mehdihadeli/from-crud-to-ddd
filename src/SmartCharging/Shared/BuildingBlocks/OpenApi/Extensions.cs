using Scalar.AspNetCore;
using SmartCharging.Shared.BuildingBlocks.Extensions;

namespace SmartCharging.Shared.BuildingBlocks.OpenApi;

// https://github.com/dotnet/aspnet-api-versioning/issues/1115

public static class Extensions
{
    public static WebApplicationBuilder AddAspnetOpenApi(this WebApplicationBuilder builder, string[] versions)
    {
        var openApiDocumentOptions = builder.Configuration.BindOptions<OpenApiDocumentOptions>();

        foreach (var documentName in versions)
        {
            builder.Services.AddOpenApi(
                documentName,
                options =>
                {
                    options.ApplyApiVersionInfo(openApiDocumentOptions);
                    options.ApplySchemaNullableFalse();
                }
            );
        }

        return builder;
    }

    public static WebApplication UseAspnetOpenApi(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return app;
        }

        app.MapOpenApi();

        app.UseSwaggerUI(options =>
        {
            var descriptions = app.DescribeApiVersions();

            // build a swagger endpoint for each discovered API version
            foreach (var description in descriptions)
            {
                var openApiUrl = $"/openapi/{description.GroupName}.json";
                var name = description.GroupName.ToUpperInvariant();
                options.SwaggerEndpoint(openApiUrl, name);
            }
        });

        // Add scalar ui
        app.MapScalarApiReference(redocOptions =>
        {
            redocOptions.WithOpenApiRoutePattern("/openapi/{documentName}.json");
        });

        app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

        return app;
    }
}
