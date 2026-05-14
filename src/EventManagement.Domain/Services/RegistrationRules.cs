using EventManagement.Domain.Entities;
using EventManagement.Domain.Exceptions;

namespace EventManagement.Domain.Services;

public sealed class RegistrationRules
{
    public void EnsureEventIsNotInPast(Event ev, DateTimeOffset now)
    {
        if (ev.Date < now)
            throw new EventInPastException();
    }

    public void EnsureCapacityAvailable(Event ev, int currentRegistrationCount)
    {
        if (currentRegistrationCount >= ev.MaxCapacity)
            throw new EventCapacityExceededException();
    }

    public void EnsureUserNotAlreadyRegistered(string userId, IEnumerable<Registration> registrations)
    {
        if (registrations.Any(r => r.UserId == userId))
            throw new DuplicateRegistrationException();
    }
}
