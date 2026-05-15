using System.ComponentModel.DataAnnotations;

namespace EventManagement.Api.Models.Events;

/// <summary>
/// HTTP request body for <c>POST /api/v1/events</c>. DataAnnotations on the properties are
/// evaluated by the <c>[ApiController]</c> filter and produce a 400 with
/// <c>ValidationProblemDetails</c> automatically — no service code runs if validation fails.
/// </summary>
public sealed class CreateEventRequest
{
    /// <summary>Event title. Required; 1–200 characters after trimming.</summary>
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Optional long-form description. Max 2000 characters.</summary>
    [StringLength(2000)]
    public string? Description { get; set; }

    /// <summary>Event date/time. Required. Nullable so a missing field becomes a clean validation error rather than defaulting to <see cref="DateTimeOffset.MinValue"/>.</summary>
    [Required]
    public DateTimeOffset? Date { get; set; }

    /// <summary>Maximum number of registrations. Must be at least 1.</summary>
    [Range(1, int.MaxValue)]
    public int MaxCapacity { get; set; }
}
