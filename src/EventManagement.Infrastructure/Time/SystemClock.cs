using EventManagement.Application.Interfaces;

namespace EventManagement.Infrastructure.Time;

/// <summary>
/// Production <see cref="IClock"/> implementation that delegates to <see cref="DateTimeOffset.UtcNow"/>.
/// Tests substitute <c>FixedClock</c> (in <c>Api.Tests/Helpers</c>) so timestamps and the
/// "event in the past" rule are deterministic.
/// </summary>
public sealed class SystemClock : IClock
{
    /// <inheritdoc />
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
