using System.ComponentModel.DataAnnotations;

namespace EventManagement.Api.Models.Registrations;

/// <summary>
/// HTTP request body for <c>POST /api/v1/events/{eventId}/registrations</c>. Carries the user
/// identity in the body because authentication is out of scope for this exercise (v1 §7) —
/// in a real system these fields would be sourced from the auth context.
/// </summary>
public sealed class RegisterUserRequest
{
    /// <summary>External user identifier. Required; 1–100 characters.</summary>
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string UserId { get; set; } = string.Empty;

    /// <summary>Display name shown alongside the registration. Required; 1–200 characters.</summary>
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string UserName { get; set; } = string.Empty;
}
