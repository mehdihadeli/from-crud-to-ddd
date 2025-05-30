namespace SmartCharging.Shared.BuildingBlocks.ProblemDetails;

public static class Extensions
{
    public static WebApplicationBuilder AddCustomProblemDetails(this WebApplicationBuilder builder)
    {
        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();

        return builder;
    }
}
