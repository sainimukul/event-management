using EventManagement.Api.Models;
using EventManagement.Api.Models.Registrations;
using EventManagement.Application.Registrations;
using EventManagement.Application.Registrations.DTOs;
using EventManagement.Application.Registrations.Requests;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.Api.Controllers;

/// <summary>
/// HTTP endpoints for managing event registrations. The event id is part of the route, so the
/// resource path always reflects the parent-child relationship.
/// </summary>
[ApiController]
[Route("api/v1/events/{eventId:guid}/registrations")]
[Produces("application/json")]
public sealed class RegistrationsController : ControllerBase
{
    private readonly RegistrationService _registrations;

    /// <summary>Creates the controller with its service dependency injected.</summary>
    /// <param name="registrations">Application service for registration lifecycle.</param>
    public RegistrationsController(RegistrationService registrations) => _registrations = registrations;

    /// <summary>Lists all registrations for a single event, in insertion order.</summary>
    /// <param name="eventId">The event whose registrations to list.</param>
    /// <response code="200">Registrations returned (possibly empty).</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RegistrationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(Guid eventId) =>
        Ok(await _registrations.ListForEventAsync(eventId));

    /// <summary>
    /// Registers a user for an event. The user identity comes from the body because authentication
    /// is out of scope for this exercise (v1 §7).
    /// </summary>
    /// <param name="eventId">Target event id.</param>
    /// <param name="req">User identity payload.</param>
    /// <response code="201">Registration created.</response>
    /// <response code="400">Request body failed DataAnnotations validation.</response>
    /// <response code="404">Target event does not exist.</response>
    /// <response code="422">A business rule was violated — past-date, at-capacity, or duplicate user.</response>
    [HttpPost]
    [ProducesResponseType(typeof(RegistrationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Register(Guid eventId, [FromBody] RegisterUserRequest req)
    {
        var input = new RegisterUserInput(req.UserId, req.UserName);
        var created = await _registrations.RegisterAsync(eventId, input);
        return Created($"/api/v1/events/{eventId}/registrations/{created.Id}", created);
    }

    /// <summary>Removes a registration.</summary>
    /// <param name="eventId">Event the registration belongs to.</param>
    /// <param name="registrationId">Registration to remove.</param>
    /// <response code="204">Registration removed.</response>
    /// <response code="404">No such registration under that event.</response>
    [HttpDelete("{registrationId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unregister(Guid eventId, Guid registrationId)
    {
        await _registrations.UnregisterAsync(eventId, registrationId);
        return NoContent();
    }
}
