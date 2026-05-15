namespace EventManagement.Domain.Exceptions;

/// <summary>
/// Thrown when a user attempts to register for an event whose date has already passed
/// (using the current <c>IClock</c> value, not <c>DateTime.UtcNow</c>).
/// Enforces requirement v1 §3.1 ("Cannot register for past events"). Maps to HTTP 422.
/// </summary>
public sealed class EventInPastException : DomainException
{
    /// <summary>Creates the exception with the standard rule message.</summary>
    public EventInPastException()
        : base("Cannot register for an event that has already occurred.") { }
}
