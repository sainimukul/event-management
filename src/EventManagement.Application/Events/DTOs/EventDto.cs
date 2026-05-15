namespace EventManagement.Application.Events.DTOs;

public sealed record EventDto(
    Guid Id,
    string Title,
    DateTimeOffset Date,
    int MaxCapacity,
    int CurrentRegistrations,
    DateTimeOffset CreatedAt);
