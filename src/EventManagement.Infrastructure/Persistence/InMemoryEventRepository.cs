using System.Collections.Concurrent;
using EventManagement.Application.Interfaces;
using EventManagement.Domain.Entities;

namespace EventManagement.Infrastructure.Persistence;

public sealed class InMemoryEventRepository : IEventRepository
{
    private readonly ConcurrentDictionary<Guid, Event> _store = new();

    public Task<IReadOnlyList<Event>> GetAllAsync() =>
        Task.FromResult<IReadOnlyList<Event>>(_store.Values.OrderBy(e => e.Date).ToList());

    public Task<Event?> GetByIdAsync(Guid id)
    {
        _store.TryGetValue(id, out var ev);
        return Task.FromResult(ev);
    }

    public Task AddAsync(Event ev)
    {
        _store[ev.Id] = ev;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Event ev)
    {
        _store[ev.Id] = ev;
        return Task.CompletedTask;
    }
}
