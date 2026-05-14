using EventManagement.Domain.Entities;

namespace EventManagement.Application.Interfaces;

public interface IRegistrationRepository
{
    Task<IReadOnlyList<Registration>> GetByEventIdAsync(Guid eventId);
    Task<Registration?> GetByIdAsync(Guid id);
    Task AddAsync(Registration registration);
    Task DeleteAsync(Guid registrationId);
    Task<bool> ExistsAsync(Guid eventId, string userId);
    Task<int> CountByEventIdAsync(Guid eventId);
}
