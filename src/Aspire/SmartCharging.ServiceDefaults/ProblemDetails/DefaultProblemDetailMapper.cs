using SmartCharging.ServiceDefaults.Exceptions;

namespace SmartCharging.ServiceDefaults.ProblemDetails;

internal sealed class DefaultProblemDetailMapper
{
    public int GetMappedStatusCodes(Exception? exception)
    {
        return exception switch
        {
            ConflictException conflictException => conflictException.StatusCode,
            ValidationException validationException => validationException.StatusCode,
            BadRequestException badRequestException => badRequestException.StatusCode,
            NotFoundException notFoundException => notFoundException.StatusCode,
            HttpResponseException httpResponseException => httpResponseException.StatusCode,
            HttpRequestException httpRequestException => (int)httpRequestException.StatusCode,
            BadHttpRequestException => StatusCodes.Status400BadRequest,
            ArgumentOutOfRangeException => StatusCodes.Status400BadRequest,
            OperationCanceledException => StatusCodes.Status499ClientClosedRequest,
            AppException appException => appException.StatusCode,
            DomainException domainException => domainException.StatusCode,
            ArgumentException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError,
        };
    }
}
