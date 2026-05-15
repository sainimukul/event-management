using EventManagement.Domain.Entities;
using EventManagement.Domain.Exceptions;

namespace EventManagement.Domain.Services;

/// <summary>
/// Pure-function business rules for registration. Each method throws a domain exception
/// when the rule is violated and returns silently when it passes — this keeps the Application
/// service's flow linear (call rule; continue) and gives the exception middleware a single,
/// uniform mapping from rule type to HTTP status.
/// </summary>
/// <remarks>
/// Time enters as a parameter rather than via <c>DateTime.UtcNow</c> so the rules stay
/// deterministically testable; production wiring sources <c>now</c> from <c>IClock</c>.
/// </remarks>
public sealed class RegistrationRules
{
    /// <summary>
    /// Rejects registration for events whose <see cref="Event.Date"/> is earlier than <paramref name="now"/>.
    /// </summary>
    /// <param name="ev">The event being registered against.</param>
    /// <param name="now">Current time, sourced from <c>IClock</c> in production.</param>
    /// <exception cref="EventInPastException">The event date has already passed.</exception>
    public void EnsureEventIsNotInPast(Event ev, DateTimeOffset now)
    {
        if (ev.Date < now)
            throw new EventInPastException();
    }

    /// <summary>
    /// Rejects registration when the existing registration count meets or exceeds capacity.
    /// </summary>
    /// <param name="ev">The event being registered against.</param>
    /// <param name="currentRegistrationCount">Count snapshot read inside the per-event lock so two concurrent registrations cannot both pass.</param>
    /// <exception cref="EventCapacityExceededException">The event is full.</exception>
    public void EnsureCapacityAvailable(Event ev, int currentRegistrationCount)
    {
        if (currentRegistrationCount >= ev.MaxCapacity)
            throw new EventCapacityExceededException();
    }

    /// <summary>
    /// Rejects registration when <paramref name="userId"/> already appears in <paramref name="registrations"/>.
    /// </summary>
    /// <param name="userId">The user attempting to register.</param>
    /// <param name="registrations">Existing registrations for the target event.</param>
    /// <exception cref="DuplicateRegistrationException">The user is already registered for the event.</exception>
    public void EnsureUserNotAlreadyRegistered(string userId, IEnumerable<Registration> registrations)
    {
        if (registrations.Any(r => r.UserId == userId))
            throw new DuplicateRegistrationException();
    }
}
