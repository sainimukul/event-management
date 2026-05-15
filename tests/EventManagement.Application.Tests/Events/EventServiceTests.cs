using EventManagement.Application.Events;
using EventManagement.Application.Events.Requests;
using EventManagement.Application.Interfaces;
using EventManagement.Domain.Entities;
using EventManagement.Domain.Exceptions;
using FluentAssertions;
using NSubstitute;

namespace EventManagement.Application.Tests.Events;

public class EventServiceTests
{
    private readonly IEventRepository _events = Substitute.For<IEventRepository>();
    private readonly IRegistrationRepository _registrations = Substitute.For<IRegistrationRepository>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly EventService _sut;

    public EventServiceTests()
    {
        _clock.UtcNow.Returns(new DateTimeOffset(2026, 5, 14, 12, 0, 0, TimeSpan.Zero));
        _sut = new EventService(_events, _registrations, _clock);
    }

    [Fact]
    public async Task CreateAsync_persists_event_and_returns_dto_with_generated_id_and_timestamps()
    {
        var input = new CreateEventInput("Conf", "Desc", new DateTimeOffset(2026, 6, 1, 9, 0, 0, TimeSpan.Zero), 50);

        var result = await _sut.CreateAsync(input);

        result.Id.Should().NotBeEmpty();
        result.Title.Should().Be("Conf");
        result.MaxCapacity.Should().Be(50);
        result.CurrentRegistrations.Should().Be(0);
        result.CreatedAt.Should().Be(_clock.UtcNow);
        await _events.Received(1).AddAsync(Arg.Is<Event>(e => e.Title == "Conf" && e.MaxCapacity == 50));
    }

    [Fact]
    public async Task UpdateAsync_throws_when_event_does_not_exist()
    {
        _events.GetByIdAsync(Arg.Any<Guid>()).Returns((Event?)null);
        var input = new UpdateEventInput("New", null, new DateTimeOffset(2026, 7, 1, 9, 0, 0, TimeSpan.Zero), 20);

        var act = () => _sut.UpdateAsync(Guid.NewGuid(), input);

        await act.Should().ThrowAsync<EventNotFoundException>();
    }

    [Fact]
    public async Task GetByIdAsync_returns_detail_dto_with_current_registration_count()
    {
        var id = Guid.NewGuid();
        var ev = new Event
        {
            Id = id,
            Title = "Conf",
            Description = "Desc",
            Date = new DateTimeOffset(2026, 6, 1, 9, 0, 0, TimeSpan.Zero),
            MaxCapacity = 50,
            CreatedAt = _clock.UtcNow,
            UpdatedAt = _clock.UtcNow,
        };
        _events.GetByIdAsync(id).Returns(ev);
        _registrations.CountByEventIdAsync(id).Returns(7);

        var result = await _sut.GetByIdAsync(id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.CurrentRegistrations.Should().Be(7);
        result.Description.Should().Be("Desc");
    }
}
