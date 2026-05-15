using EventManagement.Api.Models.Events;
using EventManagement.Application.Events;
using EventManagement.Application.Events.Requests;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.Api.Controllers;

[ApiController]
[Route("api/v1/events")]
public sealed class EventsController : ControllerBase
{
    private readonly EventService _events;

    public EventsController(EventService events) => _events = events;

    [HttpGet]
    public async Task<IActionResult> List() =>
        Ok(await _events.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var ev = await _events.GetByIdAsync(id);
        return ev is null ? NotFound() : Ok(ev);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEventRequest req)
    {
        var input = new CreateEventInput(req.Title, req.Description, req.Date!.Value, req.MaxCapacity);
        var created = await _events.CreateAsync(input);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEventRequest req)
    {
        var input = new UpdateEventInput(req.Title, req.Description, req.Date!.Value, req.MaxCapacity);
        var updated = await _events.UpdateAsync(id, input);
        return Ok(updated);
    }
}
