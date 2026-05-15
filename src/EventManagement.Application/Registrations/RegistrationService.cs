using System.Collections.Concurrent;
using EventManagement.Application.Interfaces;
using EventManagement.Application.Registrations.DTOs;
using EventManagement.Application.Registrations.Requests;
using EventManagement.Domain.Entities;
using EventManagement.Domain.Exceptions;
using EventManagement.Domain.Services;

namespace EventManagement.Application.Registrations;

/// <summary>
/// Application service for registration lifecycle. The single subtlety is concurrency:
/// without serialisation, two simultaneous registrations against the same near-full event
/// could both observe <c>count &lt; MaxCapacity</c> and both succeed, overshooting capacity.
/// A per-event <see cref="SemaphoreSlim"/> serialises the count-check-insert sequence for one
/// event at a time, while registrations to *different* events still run concurrently.
/// </summary>
public sealed class RegistrationService
{
    private readonly IEventRepository _events;
    private readonly IRegistrationRepository _registrations;
    private readonly RegistrationRules _rules;
    private readonly IClock _clock;
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _eventLocks = new();

    /// <summary>Creates a new service. Dependencies are injected by the DI container.</summary>
    /// <param name="events">Event repository, used for existence + date checks.</param>
    /// <param name="registrations">Registration repository.</param>
    /// <param name="rules">Pure-function rule evaluator.</param>
    /// <param name="clock">Clock abstraction for deterministic timestamps in tests.</param>
    public RegistrationService(
        IEventRepository events,
        IRegistrationRepository registrations,
        RegistrationRules rules,
        IClock clock)
    {
        _events = events;
        _registrations = registrations;
        _rules = rules;
        _clock = clock;
    }

    /// <summary>
    /// Registers a user for an event. Acquires the per-event lock before reading the
    /// registration count and inserting, so capacity, duplicate-user, and past-date rules
    /// can never race.
    /// </summary>
    /// <param name="eventId">Target event id.</param>
    /// <param name="input">User identity payload.</param>
    /// <returns>The created registration as a DTO.</returns>
    /// <exception cref="EventNotFoundException">Target event does not exist.</exception>
    /// <exception cref="EventInPastException">Event date has already passed.</exception>
    /// <exception cref="EventCapacityExceededException">Event is at capacity.</exception>
    /// <exception cref="DuplicateRegistrationException">User is already registered for the event.</exception>
    public async Task<RegistrationDto> RegisterAsync(Guid eventId, RegisterUserInput input)
    {
        var ev = await _events.GetByIdAsync(eventId) ?? throw new EventNotFoundException(eventId);

        var gate = _eventLocks.GetOrAdd(eventId, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync();
        try
        {
            var existing = await _registrations.GetByEventIdAsync(eventId);
            _rules.EnsureEventIsNotInPast(ev, _clock.UtcNow);
            _rules.EnsureCapacityAvailable(ev, existing.Count);
            _rules.EnsureUserNotAlreadyRegistered(input.UserId, existing);

            var registration = new Registration
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                UserId = input.UserId,
                UserName = input.UserName,
                RegisteredAt = _clock.UtcNow,
            };
            await _registrations.AddAsync(registration);
            return ToDto(registration);
        }
        finally
        {
            gate.Release();
        }
    }

    /// <summary>
    /// Removes a registration. Validates that the registration both exists and belongs to the
    /// supplied event id — a registration row found under a different event is treated as
    /// "not found" so URL tampering can't leak existence of unrelated registrations.
    /// </summary>
    /// <param name="eventId">Event the caller claims the registration belongs to.</param>
    /// <param name="registrationId">Registration id to remove.</param>
    /// <exception cref="RegistrationNotFoundException">No such registration under that event.</exception>
    public async Task UnregisterAsync(Guid eventId, Guid registrationId)
    {
        var existing = await _registrations.GetByIdAsync(registrationId)
            ?? throw new RegistrationNotFoundException(registrationId);
        if (existing.EventId != eventId)
            throw new RegistrationNotFoundException(registrationId);
        await _registrations.DeleteAsync(registrationId);
    }

    /// <summary>Lists all registrations for an event, in insertion order.</summary>
    /// <param name="eventId">Event whose registrations to list.</param>
    public async Task<IReadOnlyList<RegistrationDto>> ListForEventAsync(Guid eventId)
    {
        var registrations = await _registrations.GetByEventIdAsync(eventId);
        return registrations.Select(ToDto).ToList();
    }

    private static RegistrationDto ToDto(Registration r) =>
        new(r.Id, r.EventId, r.UserId, r.UserName, r.RegisteredAt);
}
