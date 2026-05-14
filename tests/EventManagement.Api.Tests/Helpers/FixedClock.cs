using EventManagement.Application.Interfaces;

namespace EventManagement.Api.Tests.Helpers;

public sealed class FixedClock : IClock
{
    public DateTimeOffset UtcNow { get; set; } = new(2026, 5, 14, 12, 0, 0, TimeSpan.Zero);
}
