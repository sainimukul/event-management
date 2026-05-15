namespace EventManagement.Domain.Entities;

/// <summary>
/// A single user's registration against an <see cref="Event"/>. Immutable after creation —
/// to undo a registration the row is deleted rather than mutated.
/// </summary>
public sealed class Registration
{
    /// <summary>Stable identifier for the registration row.</summary>
    public Guid Id { get; init; }

    /// <summary>The event this registration belongs to.</summary>
    public Guid EventId { get; init; }

    /// <summary>
    /// External identifier of the registering user. Treated as opaque by the domain — uniqueness
    /// of (EventId, UserId) is enforced by <see cref="Services.RegistrationRules"/>.
    /// </summary>
    public string UserId { get; init; } = string.Empty;

    /// <summary>Display name supplied with the registration. Shown to event organisers.</summary>
    public string UserName { get; init; } = string.Empty;

    /// <summary>Timestamp the registration was accepted. Sourced from <c>IClock</c>, not <c>DateTime.UtcNow</c>.</summary>
    public DateTimeOffset RegisteredAt { get; init; }
}
