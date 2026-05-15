using EventManagement.Domain.Entities;

namespace EventManagement.Application.Interfaces;

/// <summary>
/// Persistence boundary for <see cref="Registration"/> rows. The two distinct count methods
/// (<see cref="CountByEventIdAsync"/> for a single event, <see cref="CountAllByEventAsync"/>
/// for the list view) exist to keep the list endpoint O(events) instead of O(events × queries).
/// </summary>
public interface IRegistrationRepository
{
    /// <summary>Returns all registrations for a given event, in insertion order.</summary>
    /// <param name="eventId">The event whose registrations to list.</param>
    Task<IReadOnlyList<Registration>> GetByEventIdAsync(Guid eventId);

    /// <summary>Returns a single registration by id, or <c>null</c> if none exists.</summary>
    /// <param name="id">Stable identifier of the registration row.</param>
    Task<Registration?> GetByIdAsync(Guid id);

    /// <summary>Inserts a new registration. The Application service enforces uniqueness rules first.</summary>
    /// <param name="registration">The registration to insert.</param>
    Task AddAsync(Registration registration);

    /// <summary>Removes the registration with the given id. No-op if it doesn't exist.</summary>
    /// <param name="registrationId">Id of the registration to remove.</param>
    Task DeleteAsync(Guid registrationId);

    /// <summary>Returns the number of registrations for a single event.</summary>
    /// <param name="eventId">The event whose count to retrieve.</param>
    Task<int> CountByEventIdAsync(Guid eventId);

    /// <summary>Returns a map of event id → registration count for all events. Used by the events list endpoint to avoid an N+1 query.</summary>
    Task<IReadOnlyDictionary<Guid, int>> CountAllByEventAsync();
}
