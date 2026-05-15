using System.Collections.Concurrent;
using EventManagement.Application.Interfaces;
using EventManagement.Domain.Entities;

namespace EventManagement.Infrastructure.Persistence;

/// <summary>
/// Process-local, thread-safe implementation of <see cref="IEventRepository"/> backed by a
/// <see cref="ConcurrentDictionary{TKey,TValue}"/>. State is lost on process restart — per
/// requirement v1 §4 ("Use in-memory data structures"), this is intentional. Reads project a
/// fresh list on each call so callers cannot mutate the internal store.
/// </summary>
public sealed class InMemoryEventRepository : IEventRepository
{
    private readonly ConcurrentDictionary<Guid, Event> _store = new();

    /// <inheritdoc />
    public Task<IReadOnlyList<Event>> GetAllAsync() =>
        Task.FromResult<IReadOnlyList<Event>>(_store.Values.OrderBy(e => e.Date).ToList());

    /// <inheritdoc />
    public Task<Event?> GetByIdAsync(Guid id)
    {
        _store.TryGetValue(id, out var ev);
        return Task.FromResult(ev);
    }

    /// <inheritdoc />
    public Task AddAsync(Event ev)
    {
        _store[ev.Id] = ev;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task UpdateAsync(Event ev)
    {
        _store[ev.Id] = ev;
        return Task.CompletedTask;
    }
}
