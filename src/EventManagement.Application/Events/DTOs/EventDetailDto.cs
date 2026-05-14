namespace EventManagement.Application.Events.DTOs;

public sealed record EventDetailDto(
    Guid Id,
    string Title,
    string? Description,
    DateTimeOffset Date,
    int MaxCapacity,
    int CurrentRegistrations,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
