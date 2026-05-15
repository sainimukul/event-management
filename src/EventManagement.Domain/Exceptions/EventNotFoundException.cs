namespace EventManagement.Domain.Exceptions;

public sealed class EventNotFoundException : DomainException
{
    public EventNotFoundException(Guid eventId)
        : base($"Event {eventId} was not found.") { }
}
