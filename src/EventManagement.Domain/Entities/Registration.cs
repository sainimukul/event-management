namespace EventManagement.Domain.Entities;

public sealed class Registration
{
    public Guid Id { get; init; }
    public Guid EventId { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public DateTimeOffset RegisteredAt { get; init; }
}
