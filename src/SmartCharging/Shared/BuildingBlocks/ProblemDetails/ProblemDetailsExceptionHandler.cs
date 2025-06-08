using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using SmartCharging.Shared.BuildingBlocks.Exceptions;

namespace SmartCharging.Shared.BuildingBlocks.ProblemDetails;

// ref: https://anthonygiretti.com/2023/06/14/asp-net-core-8-improved-exception-handling-with-iexceptionhandler/
public class ProblemDetailsExceptionHandler(
    IProblemDetailsService problemDetailsService,
    IWebHostEnvironment webHostEnvironment,
    ILogger<ProblemDetailsExceptionHandler> logger
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        logger.LogError(exception, "Unhandled exception occurred.");

        if (context.Response.HasStarted)
        {
            logger.LogError(exception, "Response has already started, can't write response.");
            return false;
        }

        var statusCode = GetMappedStatusCodes(exception);
        var problemDetails = PopulateNewProblemDetail(statusCode, context, exception);

        context.Response.StatusCode = statusCode;

        await problemDetailsService.WriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = context,
                Exception = exception,
                ProblemDetails = problemDetails,
            }
        );

        return true;
    }

    private Microsoft.AspNetCore.Mvc.ProblemDetails PopulateNewProblemDetail(
        int code,
        HttpContext httpContext,
        Exception exception
    )
    {
        // .NET core will fill type property automatically
        var problem = TypedResults
            .Problem(
                statusCode: code,
                detail: exception.Message,
                title: exception.GetType().Name,
                instance: $"{httpContext.Request.Method} {httpContext.Request.Path}",
                extensions: webHostEnvironment.IsDevelopment()
                    ? new Dictionary<string, object?> { { "stackTrace", exception.StackTrace } }
                    : null
            )
            .ProblemDetails;

        return problem;
    }

    private static int GetMappedStatusCodes(Exception exception)
    {
        return exception switch
        {
            ConflictException conflictException => conflictException.StatusCode,
            ValidationException validationException => validationException.StatusCode,
            BadRequestException badRequestException => badRequestException.StatusCode,
            NotFoundException notFoundException => notFoundException.StatusCode,
            HttpResponseException httpResponseException => httpResponseException.StatusCode,
            HttpRequestException httpRequestException => (int)httpRequestException.StatusCode,
            DbUpdateConcurrencyException => StatusCodes.Status409Conflict,
            DbUpdateException => StatusCodes.Status500InternalServerError,
            BadHttpRequestException => StatusCodes.Status400BadRequest,
            ArgumentNullException => StatusCodes.Status400BadRequest,
            ArgumentOutOfRangeException => StatusCodes.Status400BadRequest,
            OperationCanceledException => StatusCodes.Status499ClientClosedRequest,
            AppException appException => appException.StatusCode,
            DomainException domainException => domainException.StatusCode,
            ArgumentException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError,
        };
    }
}
