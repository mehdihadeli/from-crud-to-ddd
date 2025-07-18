namespace SmartCharging.Shared.BuildingBlocks.Exceptions;

public class ConflictException(string message, System.Exception? innerException = null)
    : CustomException(message, StatusCodes.Status409Conflict, innerException);

public class ConflictAppException(string message, System.Exception? innerException = null)
    : AppException(message, StatusCodes.Status409Conflict, innerException);

public class ConflictDomainException : DomainException
{
    public ConflictDomainException(string message)
        : base(message, StatusCodes.Status409Conflict) { }

    public ConflictDomainException(Type businessRuleType, string message)
        : base(businessRuleType, message, StatusCodes.Status409Conflict) { }
}
