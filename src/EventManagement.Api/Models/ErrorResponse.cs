namespace EventManagement.Api.Models;

/// <summary>
/// Uniform error shape returned for 404, 422, and 500 responses. 400 responses use ASP.NET
/// Core's <c>ValidationProblemDetails</c> instead — the two shapes are intentionally distinct
/// so callers can tell field validation apart from business rule failures.
/// </summary>
/// <param name="Message">Human-readable error message.</param>
/// <param name="StatusCode">HTTP status code mirrored in the body for client convenience.</param>
/// <param name="Details">Optional additional detail; null on most responses.</param>
public sealed record ErrorResponse(string Message, int StatusCode, string? Details = null);
