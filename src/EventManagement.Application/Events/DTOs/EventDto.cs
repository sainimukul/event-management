namespace EventManagement.Application.Events.DTOs;

/// <summary>
/// Lightweight event projection returned by list and create endpoints. Excludes the long-form
/// description and <c>UpdatedAt</c> — use <see cref="EventDetailDto"/> when those are needed.
/// </summary>
/// <param name="Id">Stable event identifier.</param>
/// <param name="Title">Display title.</param>
/// <param name="Date">Event date/time, UTC.</param>
/// <param name="MaxCapacity">Maximum number of registrations the event accepts.</param>
/// <param name="CurrentRegistrations">Current registration count at the time of the query.</param>
/// <param name="CreatedAt">When the event was created.</param>
public sealed record EventDto(
    Guid Id,
    string Title,
    DateTimeOffset Date,
    int MaxCapacity,
    int CurrentRegistrations,
    DateTimeOffset CreatedAt);
