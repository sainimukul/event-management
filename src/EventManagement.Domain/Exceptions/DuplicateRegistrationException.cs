namespace EventManagement.Domain.Exceptions;

/// <summary>
/// Thrown when a user attempts to register for an event they are already registered for.
/// Enforces requirement v1 §3.3 ("Cannot double-register same user for same event"). Maps to HTTP 422.
/// </summary>
public sealed class DuplicateRegistrationException : DomainException
{
    /// <summary>Creates the exception with the standard rule message.</summary>
    public DuplicateRegistrationException()
        : base("User is already registered for this event.") { }
}
