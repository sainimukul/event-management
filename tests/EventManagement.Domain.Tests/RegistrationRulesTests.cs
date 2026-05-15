using EventManagement.Domain.Entities;
using EventManagement.Domain.Exceptions;
using EventManagement.Domain.Services;
using FluentAssertions;

namespace EventManagement.Domain.Tests;

public class RegistrationRulesTests
{
    private readonly DateTimeOffset _now = new(2026, 5, 14, 12, 0, 0, TimeSpan.Zero);
    private readonly RegistrationRules _rules = new();

    private Event MakeEvent(DateTimeOffset date, int capacity = 10) => new()
    {
        Id = Guid.NewGuid(),
        Title = "Test",
        Date = date,
        MaxCapacity = capacity,
        CreatedAt = _now,
        UpdatedAt = _now,
    };

    [Fact]
    public void All_rules_pass_for_future_event_with_capacity_and_new_user()
    {
        var ev = MakeEvent(_now.AddDays(1));
        var act = () =>
        {
            _rules.EnsureEventIsNotInPast(ev, _now);
            _rules.EnsureCapacityAvailable(ev, currentRegistrationCount: 5);
            _rules.EnsureUserNotAlreadyRegistered("user-1", Array.Empty<Registration>());
        };

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureEventIsNotInPast_throws_when_event_date_is_before_now()
    {
        var ev = MakeEvent(_now.AddMinutes(-1));
        var act = () => _rules.EnsureEventIsNotInPast(ev, _now);

        act.Should().Throw<EventInPastException>();
    }

    [Fact]
    public void EnsureEventIsNotInPast_does_not_throw_when_event_date_equals_now()
    {
        var ev = MakeEvent(_now);
        var act = () => _rules.EnsureEventIsNotInPast(ev, _now);

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureCapacityAvailable_throws_when_current_count_equals_max_capacity()
    {
        var ev = MakeEvent(_now.AddDays(1), capacity: 3);
        var act = () => _rules.EnsureCapacityAvailable(ev, currentRegistrationCount: 3);

        act.Should().Throw<EventCapacityExceededException>();
    }

    [Fact]
    public void EnsureCapacityAvailable_does_not_throw_when_one_slot_remains()
    {
        var ev = MakeEvent(_now.AddDays(1), capacity: 3);
        var act = () => _rules.EnsureCapacityAvailable(ev, currentRegistrationCount: 2);

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureUserNotAlreadyRegistered_throws_when_user_has_existing_registration()
    {
        var existing = new Registration
        {
            Id = Guid.NewGuid(),
            EventId = Guid.NewGuid(),
            UserId = "user-1",
            UserName = "Alice",
            RegisteredAt = _now,
        };

        var act = () => _rules.EnsureUserNotAlreadyRegistered("user-1", new[] { existing });

        act.Should().Throw<DuplicateRegistrationException>();
    }

    [Fact]
    public void EnsureUserNotAlreadyRegistered_does_not_throw_when_registrations_are_empty()
    {
        var act = () => _rules.EnsureUserNotAlreadyRegistered("user-1", Array.Empty<Registration>());

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureUserNotAlreadyRegistered_does_not_throw_when_only_other_users_are_registered()
    {
        var existing = new Registration
        {
            Id = Guid.NewGuid(),
            EventId = Guid.NewGuid(),
            UserId = "user-1",
            UserName = "Alice",
            RegisteredAt = _now,
        };

        var act = () => _rules.EnsureUserNotAlreadyRegistered("user-2", new[] { existing });

        act.Should().NotThrow();
    }
}
