namespace EventManagement.Domain.Entities;

public sealed class Event
{
    public Guid Id { get; init; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset Date { get; set; }
    public int MaxCapacity { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
}
