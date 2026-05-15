namespace EventManagement.Domain.Exceptions;

/// <summary>
/// Thrown when a registration would push an event past its <c>MaxCapacity</c>.
/// Enforces requirement v1 §3.2 ("Cannot exceed event capacity"). Maps to HTTP 422.
/// </summary>
public sealed class EventCapacityExceededException : DomainException
{
    /// <summary>Creates the exception with the standard rule message.</summary>
    public EventCapacityExceededException()
        : base("Event has reached its maximum capacity.") { }
}
