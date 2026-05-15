using System.Net;
using System.Net.Http.Json;
using EventManagement.Api.Tests.Helpers;
using EventManagement.Application.Events.DTOs;
using FluentAssertions;

namespace EventManagement.Api.Tests;

public class EventsControllerTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    public EventsControllerTests(ApiFactory factory) => _factory = factory;

    private object ValidCreateBody() => new
    {
        title = "Conf 2026",
        description = "Annual",
        date = new DateTimeOffset(2026, 8, 1, 9, 0, 0, TimeSpan.Zero),
        maxCapacity = 50,
    };

    [Fact]
    public async Task POST_events_with_valid_body_returns_201_with_location_header()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/events", ValidCreateBody());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        var dto = await response.Content.ReadFromJsonAsync<EventDto>();
        dto!.Title.Should().Be("Conf 2026");
    }

    [Fact]
    public async Task GET_events_unknown_id_returns_404()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/events/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task POST_events_with_empty_title_returns_400()
    {
        var client = _factory.CreateClient();
        var body = new
        {
            title = "",
            description = (string?)null,
            date = new DateTimeOffset(2026, 8, 1, 9, 0, 0, TimeSpan.Zero),
            maxCapacity = 10,
        };

        var response = await client.PostAsJsonAsync("/api/v1/events", body);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
