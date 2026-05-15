namespace EventManagement.Domain.Exceptions;

public sealed class EventCapacityExceededException : DomainException
{
    public EventCapacityExceededException()
        : base("Event has reached its maximum capacity.") { }
}
