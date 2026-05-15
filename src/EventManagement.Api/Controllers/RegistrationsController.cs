using EventManagement.Api.Models.Registrations;
using EventManagement.Application.Registrations;
using EventManagement.Application.Registrations.Requests;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.Api.Controllers;

[ApiController]
[Route("api/v1/events/{eventId:guid}/registrations")]
public sealed class RegistrationsController : ControllerBase
{
    private readonly RegistrationService _registrations;

    public RegistrationsController(RegistrationService registrations) => _registrations = registrations;

    [HttpGet]
    public async Task<IActionResult> List(Guid eventId) =>
        Ok(await _registrations.ListForEventAsync(eventId));

    [HttpPost]
    public async Task<IActionResult> Register(Guid eventId, [FromBody] RegisterUserRequest req)
    {
        var input = new RegisterUserInput(req.UserId, req.UserName);
        var created = await _registrations.RegisterAsync(eventId, input);
        return Created($"/api/v1/events/{eventId}/registrations/{created.Id}", created);
    }

    [HttpDelete("{registrationId:guid}")]
    public async Task<IActionResult> Unregister(Guid eventId, Guid registrationId)
    {
        await _registrations.UnregisterAsync(eventId, registrationId);
        return NoContent();
    }
}
