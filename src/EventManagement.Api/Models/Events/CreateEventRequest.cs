using System.ComponentModel.DataAnnotations;

namespace EventManagement.Api.Models.Events;

public sealed class CreateEventRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required]
    public DateTimeOffset? Date { get; set; }

    [Range(1, int.MaxValue)]
    public int MaxCapacity { get; set; }
}
