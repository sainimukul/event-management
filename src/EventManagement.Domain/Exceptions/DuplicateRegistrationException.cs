namespace EventManagement.Domain.Exceptions;

public sealed class DuplicateRegistrationException : DomainException
{
    public DuplicateRegistrationException()
        : base("User is already registered for this event.") { }
}
