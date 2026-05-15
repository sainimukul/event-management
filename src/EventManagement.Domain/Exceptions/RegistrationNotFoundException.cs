namespace EventManagement.Domain.Exceptions;

/// <summary>
/// Thrown when a registration id supplied by a caller does not exist (typically on
/// DELETE registration). Mapped to HTTP 404 by the API's exception middleware.
/// </summary>
public sealed class RegistrationNotFoundException : DomainException
{
    /// <summary>Creates the exception including the offending id in the message.</summary>
    /// <param name="registrationId">The id that was not found.</param>
    public RegistrationNotFoundException(Guid registrationId)
        : base($"Registration {registrationId} was not found.") { }
}
