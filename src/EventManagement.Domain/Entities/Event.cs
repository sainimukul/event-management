namespace EventManagement.Domain.Entities;

/// <summary>
/// An event that users can register to attend. Owned by the Domain layer; mutated only through
/// the Application services so business invariants stay enforceable in one place.
/// </summary>
public sealed class Event
{
    /// <summary>Stable identifier for the event. Assigned on creation and never changes.</summary>
    public Guid Id { get; init; }

    /// <summary>Human-readable title shown in listings and on the event detail page.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Optional long-form description. May be null when no description is supplied.</summary>
    public string? Description { get; set; }

    /// <summary>Date and time the event takes place. Stored as UTC; the UI converts to the user's local zone at the boundary.</summary>
    public DateTimeOffset Date { get; set; }

    /// <summary>Maximum number of registrations the event will accept. Enforced by <see cref="Services.RegistrationRules"/>.</summary>
    public int MaxCapacity { get; set; }

    /// <summary>Timestamp the event row was first created. Set once and never modified.</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Timestamp of the most recent update to the event.</summary>
    public DateTimeOffset UpdatedAt { get; set; }
}
