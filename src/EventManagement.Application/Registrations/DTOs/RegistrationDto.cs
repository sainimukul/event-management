namespace EventManagement.Application.Registrations.DTOs;

public sealed record RegistrationDto(
    Guid Id,
    Guid EventId,
    string UserId,
    string UserName,
    DateTimeOffset RegisteredAt);
