namespace EventManagement.Application.Interfaces;

/// <summary>
/// Abstraction over the system clock. The Domain layer never calls <c>DateTime.UtcNow</c>
/// directly — time enters either as a method parameter or via this interface — which keeps
/// business rules deterministically testable. Implemented by <c>SystemClock</c> in
/// Infrastructure for production and by <c>FixedClock</c> in <c>Api.Tests/Helpers</c> for tests.
/// </summary>
public interface IClock
{
    /// <summary>The current UTC instant.</summary>
    DateTimeOffset UtcNow { get; }
}
