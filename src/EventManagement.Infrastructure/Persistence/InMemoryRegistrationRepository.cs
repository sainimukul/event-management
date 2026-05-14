using System.Collections.Concurrent;
using EventManagement.Application.Interfaces;
using EventManagement.Domain.Entities;

namespace EventManagement.Infrastructure.Persistence;

public sealed class InMemoryRegistrationRepository : IRegistrationRepository
{
    private readonly ConcurrentDictionary<Guid, Registration> _store = new();

    public Task<IReadOnlyList<Registration>> GetByEventIdAsync(Guid eventId) =>
        Task.FromResult<IReadOnlyList<Registration>>(
            _store.Values.Where(r => r.EventId == eventId).OrderBy(r => r.RegisteredAt).ToList());

    public Task<Registration?> GetByIdAsync(Guid id)
    {
        _store.TryGetValue(id, out var r);
        return Task.FromResult(r);
    }

    public Task AddAsync(Registration registration)
    {
        _store[registration.Id] = registration;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid registrationId)
    {
        _store.TryRemove(registrationId, out _);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(Guid eventId, string userId) =>
        Task.FromResult(_store.Values.Any(r => r.EventId == eventId && r.UserId == userId));

    public Task<int> CountByEventIdAsync(Guid eventId) =>
        Task.FromResult(_store.Values.Count(r => r.EventId == eventId));
}
