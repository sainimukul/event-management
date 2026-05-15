namespace EventManagement.Application.Events.DTOs;

/// <summary>
/// Full event projection returned by the detail and update endpoints. Adds
/// <see cref="Description"/> and <see cref="UpdatedAt"/> on top of <see cref="EventDto"/>.
/// </summary>
/// <param name="Id">Stable event identifier.</param>
/// <param name="Title">Display title.</param>
/// <param name="Description">Optional long-form description.</param>
/// <param name="Date">Event date/time, UTC.</param>
/// <param name="MaxCapacity">Maximum number of registrations the event accepts.</param>
/// <param name="CurrentRegistrations">Current registration count at the time of the query.</param>
/// <param name="CreatedAt">When the event was created.</param>
/// <param name="UpdatedAt">When the event was last modified.</param>
public sealed record EventDetailDto(
    Guid Id,
    string Title,
    string? Description,
    DateTimeOffset Date,
    int MaxCapacity,
    int CurrentRegistrations,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
