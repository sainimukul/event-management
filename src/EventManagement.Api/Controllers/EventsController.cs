using EventManagement.Api.Models;
using EventManagement.Api.Models.Events;
using EventManagement.Application.Events;
using EventManagement.Application.Events.DTOs;
using EventManagement.Application.Events.Requests;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.Api.Controllers;

/// <summary>
/// HTTP endpoints for managing events. Versioned under <c>/api/v1/events</c>; future
/// versions live alongside under <c>/api/v2/...</c> etc.
/// </summary>
/// <remarks>
/// There is intentionally no <c>DELETE /api/v1/events/{id}</c> endpoint — requirement v1 §3
/// scopes events to Create/Read/Update only. Domain rule violations bubble up as exceptions
/// and are translated to HTTP status codes by <see cref="Middleware.ExceptionHandlingMiddleware"/>.
/// </remarks>
[ApiController]
[Route("api/v1/events")]
[Produces("application/json")]
public sealed class EventsController : ControllerBase
{
    private readonly EventService _events;

    /// <summary>Creates the controller with its service dependency injected.</summary>
    /// <param name="events">Application service for event CRUD.</param>
    public EventsController(EventService events) => _events = events;

    /// <summary>Lists all events with their current registration counts.</summary>
    /// <response code="200">Events returned, ordered by date ascending.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EventDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List() =>
        Ok(await _events.GetAllAsync());

    /// <summary>Returns the full detail of a single event.</summary>
    /// <param name="id">Stable identifier of the event.</param>
    /// <response code="200">Event detail.</response>
    /// <response code="404">No event exists with the given id.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EventDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var ev = await _events.GetByIdAsync(id);
        return ev is null ? NotFound() : Ok(ev);
    }

    /// <summary>Creates a new event.</summary>
    /// <param name="req">Event fields. <c>Title</c> is required (max 200), <c>MaxCapacity</c> must be positive, <c>Date</c> is required.</param>
    /// <response code="201">Event created. The <c>Location</c> header points to the new resource.</response>
    /// <response code="400">Request body failed DataAnnotations validation (<c>ValidationProblemDetails</c>).</response>
    [HttpPost]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateEventRequest req)
    {
        var input = new CreateEventInput(req.Title, req.Description, req.Date!.Value, req.MaxCapacity);
        var created = await _events.CreateAsync(input);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Updates an existing event in place.</summary>
    /// <param name="id">Stable identifier of the event to update.</param>
    /// <param name="req">Replacement field values.</param>
    /// <response code="200">Event updated.</response>
    /// <response code="400">Request body failed DataAnnotations validation.</response>
    /// <response code="404">No event exists with the given id.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEventRequest req)
    {
        var input = new UpdateEventInput(req.Title, req.Description, req.Date!.Value, req.MaxCapacity);
        var updated = await _events.UpdateAsync(id, input);
        return Ok(updated);
    }
}
