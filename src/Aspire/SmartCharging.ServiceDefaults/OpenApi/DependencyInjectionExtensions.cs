using Scalar.AspNetCore;
using SmartCharging.ServiceDefaults.Extensions;

namespace SmartCharging.ServiceDefaults.OpenApi;

public static class DependencyInjectionExtensions
{
    public static IHostApplicationBuilder AddAspnetOpenApi(this IHostApplicationBuilder builder, string[] versions)
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
        // show in both production and development
        app.MapGet("/", () => $"{app.Environment.ApplicationName} is started.").ExcludeFromDescription();

        if (!app.Environment.IsDevelopment())
            return app;

        // we should not see openapi docs in none development mode
        app.MapOpenApi();

        // add swagger ui
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
        app.MapScalarApiReference(scalarOptions =>
        {
            scalarOptions.WithOpenApiRoutePattern("/openapi/{documentName}.json");
            scalarOptions.Theme = ScalarTheme.BluePlanet;
            // Disable default fonts to avoid download unnecessary fonts
            scalarOptions.DefaultFonts = false;
        });

        return app;
    }
}
