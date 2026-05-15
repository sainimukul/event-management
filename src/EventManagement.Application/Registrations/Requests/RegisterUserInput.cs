namespace EventManagement.Application.Registrations.Requests;

/// <summary>
/// Pre-validated input to <c>RegistrationService.RegisterAsync</c>. Authentication is out of
/// scope for the take-home (per v1 §7), so the user identity is carried explicitly in the body
/// rather than read from a security context.
/// </summary>
/// <param name="UserId">External user identifier; treated as opaque by the domain.</param>
/// <param name="UserName">Display name shown alongside the registration.</param>
public sealed record RegisterUserInput(string UserId, string UserName);
