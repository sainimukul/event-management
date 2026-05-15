using System.ComponentModel.DataAnnotations;

namespace EventManagement.Api.Models.Events;

/// <summary>
/// HTTP request body for <c>PUT /api/v1/events/{id}</c>. Identical shape to
/// <see cref="CreateEventRequest"/> today but kept as a distinct type so future per-operation
/// validation (e.g. disallowing date changes once registrations exist) can be added cleanly.
/// </summary>
public sealed class UpdateEventRequest
{
    /// <summary>Event title. Required; 1–200 characters after trimming.</summary>
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Optional long-form description. Max 2000 characters.</summary>
    [StringLength(2000)]
    public string? Description { get; set; }

    /// <summary>Event date/time. Required.</summary>
    [Required]
    public DateTimeOffset? Date { get; set; }

    /// <summary>Maximum number of registrations. Must be at least 1.</summary>
    [Range(1, int.MaxValue)]
    public int MaxCapacity { get; set; }
}
