using EventManagement.Application.Events.DTOs;
using EventManagement.Application.Events.Requests;
using EventManagement.Application.Interfaces;
using EventManagement.Domain.Entities;
using EventManagement.Domain.Exceptions;

namespace EventManagement.Application.Events;

/// <summary>
/// Application service for event CRUD. Owns id assignment, UTC normalisation, and the join
/// to registration counts so the API controller stays a thin shell.
/// </summary>
public sealed class EventService
{
    private readonly IEventRepository _events;
    private readonly IRegistrationRepository _registrations;
    private readonly IClock _clock;

    /// <summary>Creates a new service. Dependencies are injected by the DI container.</summary>
    /// <param name="events">Event persistence boundary.</param>
    /// <param name="registrations">Registration persistence boundary, used for the count join.</param>
    /// <param name="clock">Clock abstraction so <c>CreatedAt</c>/<c>UpdatedAt</c> are deterministic in tests.</param>
    public EventService(IEventRepository events, IRegistrationRepository registrations, IClock clock)
    {
        _events = events;
        _registrations = registrations;
        _clock = clock;
    }

    /// <summary>
    /// Creates an event, trimming the title and normalising the date to UTC. Both
    /// <c>CreatedAt</c> and <c>UpdatedAt</c> are stamped with the same clock value.
    /// </summary>
    /// <param name="input">Pre-validated input.</param>
    /// <returns>The newly created event as an <see cref="EventDto"/> with zero registrations.</returns>
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

    /// <summary>
    /// Returns all events with their current registration counts. Uses a single batched count
    /// query (<see cref="IRegistrationRepository.CountAllByEventAsync"/>) to avoid an N+1.
    /// </summary>
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

    /// <summary>
    /// Returns the full event detail including description, or <c>null</c> if the event does not exist.
    /// Returning <c>null</c> (rather than throwing) lets the controller distinguish the 404 case
    /// from genuine domain errors without exception-driven control flow.
    /// </summary>
    /// <param name="id">Stable identifier of the event.</param>
    public async Task<EventDetailDto?> GetByIdAsync(Guid id)
    {
        var ev = await _events.GetByIdAsync(id);
        if (ev is null) return null;
        var count = await _registrations.CountByEventIdAsync(id);
        return new EventDetailDto(ev.Id, ev.Title, ev.Description, ev.Date, ev.MaxCapacity, count, ev.CreatedAt, ev.UpdatedAt);
    }

    /// <summary>
    /// Updates an existing event, refreshing <c>UpdatedAt</c>. Throws <see cref="EventNotFoundException"/>
    /// (mapped to HTTP 404) when the id does not exist — chosen over a nullable return because
    /// callers of update always treat "not found" as an error path.
    /// </summary>
    /// <param name="id">Stable identifier of the event.</param>
    /// <param name="input">Replacement field values.</param>
    /// <exception cref="EventNotFoundException">No event exists with the given id.</exception>
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
