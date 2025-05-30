namespace SmartCharging.Shared.BuildingBlocks.Exceptions;

public class IdentityException(
    string message,
    int statusCode = StatusCodes.Status400BadRequest,
    System.Exception? innerException = null,
    params string[] errors
) : CustomException(message, statusCode, innerException, errors);
