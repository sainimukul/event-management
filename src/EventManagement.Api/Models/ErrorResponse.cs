namespace EventManagement.Api.Models;

public sealed record ErrorResponse(string Message, int StatusCode, string? Details = null);
