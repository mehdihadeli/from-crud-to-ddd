namespace SmartCharging.Shared.Application.Extensions.WebApplicationExtensions;

public static class InfrastructureExtensions
{
    public static WebApplication UseInfrastructure(this WebApplication app)
    {
        app.UseExceptionHandler(new ExceptionHandlerOptions { AllowStatusCode404Response = true });
        // Handles non-exceptional status codes (e.g., 404 from Results.NotFound(), 401 from unauthorized access) and returns standardized ProblemDetails responses
        app.UseStatusCodePages();

        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}
