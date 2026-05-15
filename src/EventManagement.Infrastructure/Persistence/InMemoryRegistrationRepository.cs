using System.Collections.Concurrent;
using EventManagement.Application.Interfaces;
using EventManagement.Domain.Entities;

namespace EventManagement.Infrastructure.Persistence;

/// <summary>
/// Process-local, thread-safe implementation of <see cref="IRegistrationRepository"/>. The
/// store is keyed by registration id; event-scoped queries filter in-memory. For a real
/// persistence layer this would be replaced with an indexed table query.
/// </summary>
public sealed class InMemoryRegistrationRepository : IRegistrationRepository
{
    private readonly ConcurrentDictionary<Guid, Registration> _store = new();

    /// <inheritdoc />
    public Task<IReadOnlyList<Registration>> GetByEventIdAsync(Guid eventId) =>
        Task.FromResult<IReadOnlyList<Registration>>(
            _store.Values.Where(r => r.EventId == eventId).OrderBy(r => r.RegisteredAt).ToList());

    /// <inheritdoc />
    public Task<Registration?> GetByIdAsync(Guid id)
    {
        _store.TryGetValue(id, out var r);
        return Task.FromResult(r);
    }

    /// <inheritdoc />
    public Task AddAsync(Registration registration)
    {
        _store[registration.Id] = registration;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteAsync(Guid registrationId)
    {
        _store.TryRemove(registrationId, out _);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<int> CountByEventIdAsync(Guid eventId) =>
        Task.FromResult(_store.Values.Count(r => r.EventId == eventId));

    /// <inheritdoc />
    public Task<IReadOnlyDictionary<Guid, int>> CountAllByEventAsync() =>
        Task.FromResult<IReadOnlyDictionary<Guid, int>>(
            _store.Values
                .GroupBy(r => r.EventId)
                .ToDictionary(g => g.Key, g => g.Count()));
}
