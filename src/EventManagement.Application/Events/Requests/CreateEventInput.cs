namespace EventManagement.Application.Events.Requests;

public sealed record CreateEventInput(string Title, string? Description, DateTimeOffset Date, int MaxCapacity);
