namespace EventManagement.Application.Registrations.DTOs;

/// <summary>
/// Projection of a <c>Registration</c> entity returned by list and create endpoints.
/// </summary>
/// <param name="Id">Stable registration identifier.</param>
/// <param name="EventId">The event this registration is for.</param>
/// <param name="UserId">External identifier of the registering user (opaque to the API).</param>
/// <param name="UserName">Display name supplied at registration time.</param>
/// <param name="RegisteredAt">When the registration was accepted, UTC.</param>
public sealed record RegistrationDto(
    Guid Id,
    Guid EventId,
    string UserId,
    string UserName,
    DateTimeOffset RegisteredAt);
