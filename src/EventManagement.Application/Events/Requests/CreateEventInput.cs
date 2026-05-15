namespace EventManagement.Application.Events.Requests;

/// <summary>
/// Pre-validated input to <c>EventService.CreateAsync</c>. The Api layer adapts its
/// <c>CreateEventRequest</c> (DataAnnotations-validated) into this record so the Application
/// layer never sees raw HTTP shapes.
/// </summary>
/// <param name="Title">Display title; trimmed by the service.</param>
/// <param name="Description">Optional long-form description.</param>
/// <param name="Date">Event date/time. Converted to UTC by the service.</param>
/// <param name="MaxCapacity">Maximum number of registrations the event accepts.</param>
public sealed record CreateEventInput(string Title, string? Description, DateTimeOffset Date, int MaxCapacity);
