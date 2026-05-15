using System.Text.Json;
using EventManagement.Api.Models;
using EventManagement.Domain.Exceptions;

namespace EventManagement.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (EventNotFoundException ex)
        {
            await Write(context, 404, ex.Message);
        }
        catch (RegistrationNotFoundException ex)
        {
            await Write(context, 404, ex.Message);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Business rule violation on {Path}", context.Request.Path);
            await Write(context, 422, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception on {Path}", context.Request.Path);
            await Write(context, 500, "An unexpected error occurred.");
        }
    }

    private static async Task Write(HttpContext context, int status, string message)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";
        var payload = new ErrorResponse(message, status);
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}
