namespace TaskTracker.Api.Services;

/// <summary>
/// Thrown by the service layer when a requested entity does not exist.
/// Translated to a 404 ProblemDetails response by the API layer.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
}
