namespace PlanningPoker.Api.Services;

public class SessionNotFoundException : Exception
{
    public SessionNotFoundException(string sessionCode)
        : base($"Session '{sessionCode}' not found")
    {
    }
}

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message)
    {
    }
}

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message)
    {
    }
}
