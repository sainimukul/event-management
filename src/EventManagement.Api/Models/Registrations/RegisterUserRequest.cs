using System.ComponentModel.DataAnnotations;

namespace EventManagement.Api.Models.Registrations;

public sealed class RegisterUserRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string UserName { get; set; } = string.Empty;
}
