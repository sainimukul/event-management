using EventManagement.Domain.Entities;

namespace EventManagement.Application.Interfaces;

/// <summary>
/// Persistence boundary for <see cref="Event"/> aggregates. Owned by the Application layer
/// (not Infrastructure) so the domain depends on its own abstraction rather than the storage
/// detail. Returns <see cref="Task"/> rather than <c>ValueTask</c> to match the contract a
/// future EF Core or Dapper implementation would naturally expose.
/// </summary>
public interface IEventRepository
{
    /// <summary>Returns all events, typically ordered by date ascending.</summary>
    Task<IReadOnlyList<Event>> GetAllAsync();

    /// <summary>Returns the event with the given id, or <c>null</c> if no such event exists.</summary>
    /// <param name="id">Stable identifier of the event.</param>
    Task<Event?> GetByIdAsync(Guid id);

    /// <summary>Inserts a new event into the store. Caller must supply a fresh id.</summary>
    /// <param name="ev">The event to insert.</param>
    Task AddAsync(Event ev);

    /// <summary>Persists changes to an existing event identified by <see cref="Event.Id"/>.</summary>
    /// <param name="ev">The event whose fields have been mutated.</param>
    Task UpdateAsync(Event ev);
}
