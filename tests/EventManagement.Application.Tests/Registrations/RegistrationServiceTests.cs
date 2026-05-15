using EventManagement.Application.Interfaces;
using EventManagement.Application.Registrations;
using EventManagement.Application.Registrations.Requests;
using EventManagement.Domain.Entities;
using EventManagement.Domain.Exceptions;
using EventManagement.Domain.Services;
using FluentAssertions;
using NSubstitute;

namespace EventManagement.Application.Tests.Registrations;

public class RegistrationServiceTests
{
    private readonly IEventRepository _events = Substitute.For<IEventRepository>();
    private readonly IRegistrationRepository _registrations = Substitute.For<IRegistrationRepository>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly RegistrationRules _rules = new();
    private readonly RegistrationService _sut;
    private readonly DateTimeOffset _now = new(2026, 5, 14, 12, 0, 0, TimeSpan.Zero);

    public RegistrationServiceTests()
    {
        _clock.UtcNow.Returns(_now);
        _sut = new RegistrationService(_events, _registrations, _rules, _clock);
    }

    private Event MakeEvent(int capacity = 10, DateTimeOffset? date = null) => new()
    {
        Id = Guid.NewGuid(),
        Title = "Conf",
        Date = date ?? _now.AddDays(1),
        MaxCapacity = capacity,
        CreatedAt = _now,
        UpdatedAt = _now,
    };

    [Fact]
    public async Task RegisterAsync_success_persists_registration_and_returns_dto()
    {
        var ev = MakeEvent();
        _events.GetByIdAsync(ev.Id).Returns(ev);
        _registrations.GetByEventIdAsync(ev.Id).Returns(Array.Empty<Registration>());

        var input = new RegisterUserInput("user-1", "Alice");
        var result = await _sut.RegisterAsync(ev.Id, input);

        result.UserId.Should().Be("user-1");
        result.UserName.Should().Be("Alice");
        result.EventId.Should().Be(ev.Id);
        result.RegisteredAt.Should().Be(_now);
        await _registrations.Received(1).AddAsync(Arg.Is<Registration>(r => r.UserId == "user-1" && r.EventId == ev.Id));
    }

    [Fact]
    public async Task RegisterAsync_throws_when_event_does_not_exist()
    {
        _events.GetByIdAsync(Arg.Any<Guid>()).Returns((Event?)null);

        var act = () => _sut.RegisterAsync(Guid.NewGuid(), new RegisterUserInput("user-1", "Alice"));

        await act.Should().ThrowAsync<EventNotFoundException>();
    }

    [Fact]
    public async Task RegisterAsync_throws_when_user_is_already_registered()
    {
        var ev = MakeEvent();
        var existing = new Registration { Id = Guid.NewGuid(), EventId = ev.Id, UserId = "user-1", UserName = "Alice", RegisteredAt = _now };
        _events.GetByIdAsync(ev.Id).Returns(ev);
        _registrations.GetByEventIdAsync(ev.Id).Returns(new[] { existing });

        var act = () => _sut.RegisterAsync(ev.Id, new RegisterUserInput("user-1", "Alice"));

        await act.Should().ThrowAsync<DuplicateRegistrationException>();
    }

    [Fact]
    public async Task RegisterAsync_throws_when_event_is_at_capacity()
    {
        var ev = MakeEvent(capacity: 1);
        var existing = new Registration { Id = Guid.NewGuid(), EventId = ev.Id, UserId = "user-2", UserName = "Bob", RegisteredAt = _now };
        _events.GetByIdAsync(ev.Id).Returns(ev);
        _registrations.GetByEventIdAsync(ev.Id).Returns(new[] { existing });

        var act = () => _sut.RegisterAsync(ev.Id, new RegisterUserInput("user-1", "Alice"));

        await act.Should().ThrowAsync<EventCapacityExceededException>();
    }

    [Fact]
    public async Task UnregisterAsync_throws_when_registration_not_found()
    {
        _registrations.GetByIdAsync(Arg.Any<Guid>()).Returns((Registration?)null);

        var act = () => _sut.UnregisterAsync(Guid.NewGuid(), Guid.NewGuid());

        await act.Should().ThrowAsync<RegistrationNotFoundException>();
    }
}
