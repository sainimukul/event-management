using EventManagement.Application.Events.DTOs;
using EventManagement.Application.Events.Requests;
using EventManagement.Application.Interfaces;
using EventManagement.Domain.Entities;
using EventManagement.Domain.Exceptions;

namespace EventManagement.Application.Events;

public sealed class EventService
{
    private readonly IEventRepository _events;
    private readonly IRegistrationRepository _registrations;
    private readonly IClock _clock;

    public EventService(IEventRepository events, IRegistrationRepository registrations, IClock clock)
    {
        _events = events;
        _registrations = registrations;
        _clock = clock;
    }

    public async Task<EventDto> CreateAsync(CreateEventInput input)
    {
        var now = _clock.UtcNow;
        var ev = new Event
        {
            Id = Guid.NewGuid(),
            Title = input.Title.Trim(),
            Description = input.Description,
            Date = input.Date.ToUniversalTime(),
            MaxCapacity = input.MaxCapacity,
            CreatedAt = now,
            UpdatedAt = now,
        };
        await _events.AddAsync(ev);
        return new EventDto(ev.Id, ev.Title, ev.Date, ev.MaxCapacity, 0, ev.CreatedAt);
    }

    public async Task<IReadOnlyList<EventDto>> GetAllAsync()
    {
        var events = await _events.GetAllAsync();
        var counts = await _registrations.CountAllByEventAsync();
        return events
            .Select(ev => new EventDto(
                ev.Id,
                ev.Title,
                ev.Date,
                ev.MaxCapacity,
                counts.TryGetValue(ev.Id, out var c) ? c : 0,
                ev.CreatedAt))
            .ToList();
    }

    public async Task<EventDetailDto?> GetByIdAsync(Guid id)
    {
        var ev = await _events.GetByIdAsync(id);
        if (ev is null) return null;
        var count = await _registrations.CountByEventIdAsync(id);
        return new EventDetailDto(ev.Id, ev.Title, ev.Description, ev.Date, ev.MaxCapacity, count, ev.CreatedAt, ev.UpdatedAt);
    }

    public async Task<EventDto> UpdateAsync(Guid id, UpdateEventInput input)
    {
        var ev = await _events.GetByIdAsync(id) ?? throw new EventNotFoundException(id);
        ev.Title = input.Title.Trim();
        ev.Description = input.Description;
        ev.Date = input.Date.ToUniversalTime();
        ev.MaxCapacity = input.MaxCapacity;
        ev.UpdatedAt = _clock.UtcNow;
        await _events.UpdateAsync(ev);
        var count = await _registrations.CountByEventIdAsync(id);
        return new EventDto(ev.Id, ev.Title, ev.Date, ev.MaxCapacity, count, ev.CreatedAt);
    }
}
