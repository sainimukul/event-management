namespace EventManagement.Application.Events.Requests;

public sealed record UpdateEventInput(string Title, string? Description, DateTimeOffset Date, int MaxCapacity);
