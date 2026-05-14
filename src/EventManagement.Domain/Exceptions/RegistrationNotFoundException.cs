namespace EventManagement.Domain.Exceptions;

public sealed class RegistrationNotFoundException : DomainException
{
    public RegistrationNotFoundException(Guid registrationId)
        : base($"Registration {registrationId} was not found.") { }
}
