namespace SmartCharging.Shared.BuildingBlocks.Exceptions;

public class ForbiddenException : IdentityException
{
    public ForbiddenException(string message, System.Exception? innerException = null)
        : base(message, statusCode: StatusCodes.Status403Forbidden, innerException) { }
}
