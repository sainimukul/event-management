namespace EventManagement.Domain.Exceptions;

/// <summary>
/// Base type for all business-rule violations raised by the Domain layer. The API's
/// <c>ExceptionHandlingMiddleware</c> maps any <see cref="DomainException"/> not matched by a
/// more specific handler to HTTP 422 Unprocessable Entity, so new rule violations can be added
/// by simply subclassing this type.
/// </summary>
public abstract class DomainException : Exception
{
    /// <summary>Creates a domain exception carrying a human-readable rule message.</summary>
    /// <param name="message">Message returned to API callers in the error body.</param>
    protected DomainException(string message) : base(message) { }
}
