using System.Net;
using System.Net.Http.Json;
using EventManagement.Api.Tests.Helpers;
using EventManagement.Application.Events.DTOs;
using EventManagement.Application.Registrations.DTOs;
using FluentAssertions;

namespace EventManagement.Api.Tests;

public class RegistrationsControllerTests
{
    private static object CreateEventBody(DateTimeOffset date, int capacity = 10) => new
    {
        title = "Conf",
        description = (string?)null,
        date,
        maxCapacity = capacity,
    };

    private static object RegisterBody(string userId, string userName) => new { userId, userName };

    private async Task<Guid> CreateEvent(HttpClient client, DateTimeOffset date, int capacity = 10)
    {
        var response = await client.PostAsJsonAsync("/api/v1/events", CreateEventBody(date, capacity));
        response.EnsureSuccessStatusCode();
        var dto = await response.Content.ReadFromJsonAsync<EventDto>();
        return dto!.Id;
    }

    [Fact]
    public async Task POST_registration_for_future_event_returns_201()
    {
        await using var factory = new ApiFactory();
        var client = factory.CreateClient();
        var eventId = await CreateEvent(client, factory.Clock.UtcNow.AddDays(1));

        var response = await client.PostAsJsonAsync($"/api/v1/events/{eventId}/registrations",
            RegisterBody("user-1", "Alice"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var dto = await response.Content.ReadFromJsonAsync<RegistrationDto>();
        dto!.UserId.Should().Be("user-1");
    }

    [Fact]
    public async Task POST_duplicate_registration_returns_422()
    {
        await using var factory = new ApiFactory();
        var client = factory.CreateClient();
        var eventId = await CreateEvent(client, factory.Clock.UtcNow.AddDays(1));
        await client.PostAsJsonAsync($"/api/v1/events/{eventId}/registrations", RegisterBody("user-1", "Alice"));

        var response = await client.PostAsJsonAsync($"/api/v1/events/{eventId}/registrations",
            RegisterBody("user-1", "Alice"));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task POST_registration_when_at_capacity_returns_422()
    {
        await using var factory = new ApiFactory();
        var client = factory.CreateClient();
        var eventId = await CreateEvent(client, factory.Clock.UtcNow.AddDays(1), capacity: 1);
        await client.PostAsJsonAsync($"/api/v1/events/{eventId}/registrations", RegisterBody("user-1", "Alice"));

        var response = await client.PostAsJsonAsync($"/api/v1/events/{eventId}/registrations",
            RegisterBody("user-2", "Bob"));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task POST_registration_for_past_event_returns_422()
    {
        await using var factory = new ApiFactory();
        var client = factory.CreateClient();
        // Create event in the future, then move the clock past it.
        var eventDate = factory.Clock.UtcNow.AddHours(1);
        var eventId = await CreateEvent(client, eventDate);
        factory.Clock.UtcNow = eventDate.AddMinutes(1);

        var response = await client.PostAsJsonAsync($"/api/v1/events/{eventId}/registrations",
            RegisterBody("user-1", "Alice"));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
