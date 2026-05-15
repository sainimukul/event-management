namespace EventManagement.Application.Events.Requests;

/// <summary>
/// Pre-validated input to <c>EventService.UpdateAsync</c>. Same shape as
/// <see cref="CreateEventInput"/> — the two are kept distinct so future divergence (e.g.
/// disallowing date changes after the first registration) can be added without coupling.
/// </summary>
/// <param name="Title">Display title; trimmed by the service.</param>
/// <param name="Description">Optional long-form description.</param>
/// <param name="Date">Event date/time. Converted to UTC by the service.</param>
/// <param name="MaxCapacity">Maximum number of registrations the event accepts.</param>
public sealed record UpdateEventInput(string Title, string? Description, DateTimeOffset Date, int MaxCapacity);
