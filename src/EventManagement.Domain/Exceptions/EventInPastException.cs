namespace EventManagement.Domain.Exceptions;

public sealed class EventInPastException : DomainException
{
    public EventInPastException()
        : base("Cannot register for an event that has already occurred.") { }
}
