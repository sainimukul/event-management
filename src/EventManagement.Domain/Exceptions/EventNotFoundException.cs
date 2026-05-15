namespace EventManagement.Domain.Exceptions;

/// <summary>
/// Thrown when an event id supplied by a caller does not exist in the repository.
/// Caught explicitly in <c>ExceptionHandlingMiddleware</c> and mapped to HTTP 404
/// before the generic <see cref="DomainException"/> handler (which would otherwise produce 422).
/// </summary>
public sealed class EventNotFoundException : DomainException
{
    /// <summary>Creates the exception including the offending id in the message.</summary>
    /// <param name="eventId">The id that was not found.</param>
    public EventNotFoundException(Guid eventId)
        : base($"Event {eventId} was not found.") { }
}
