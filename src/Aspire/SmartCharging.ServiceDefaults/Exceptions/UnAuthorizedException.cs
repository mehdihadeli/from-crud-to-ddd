namespace SmartCharging.ServiceDefaults.Exceptions;

public class UnAuthorizedException(string message, Exception? innerException = null)
    : IdentityException(message, StatusCodes.Status401Unauthorized, innerException);
