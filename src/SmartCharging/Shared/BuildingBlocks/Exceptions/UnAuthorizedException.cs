namespace SmartCharging.Shared.BuildingBlocks.Exceptions;

public class UnAuthorizedException(string message, System.Exception? innerException = null)
    : IdentityException(message, StatusCodes.Status401Unauthorized, innerException);
