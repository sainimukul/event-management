using System.Collections.Concurrent;
using EventManagement.Application.Interfaces;
using EventManagement.Application.Registrations.DTOs;
using EventManagement.Application.Registrations.Requests;
using EventManagement.Domain.Entities;
using EventManagement.Domain.Exceptions;
using EventManagement.Domain.Services;

namespace EventManagement.Application.Registrations;

public sealed class RegistrationService
{
    private readonly IEventRepository _events;
    private readonly IRegistrationRepository _registrations;
    private readonly RegistrationRules _rules;
    private readonly IClock _clock;
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _eventLocks = new();

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

    public async Task UnregisterAsync(Guid eventId, Guid registrationId)
    {
        var existing = await _registrations.GetByIdAsync(registrationId)
            ?? throw new RegistrationNotFoundException(registrationId);
        if (existing.EventId != eventId)
            throw new RegistrationNotFoundException(registrationId);
        await _registrations.DeleteAsync(registrationId);
    }

    public async Task<IReadOnlyList<RegistrationDto>> ListForEventAsync(Guid eventId)
    {
        var registrations = await _registrations.GetByEventIdAsync(eventId);
        return registrations.Select(ToDto).ToList();
    }

    private static RegistrationDto ToDto(Registration r) =>
        new(r.Id, r.EventId, r.UserId, r.UserName, r.RegisteredAt);
}
