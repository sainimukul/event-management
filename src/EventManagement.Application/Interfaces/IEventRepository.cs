using EventManagement.Domain.Entities;

namespace EventManagement.Application.Interfaces;

public interface IEventRepository
{
    Task<IReadOnlyList<Event>> GetAllAsync();
    Task<Event?> GetByIdAsync(Guid id);
    Task AddAsync(Event ev);
    Task UpdateAsync(Event ev);
}
