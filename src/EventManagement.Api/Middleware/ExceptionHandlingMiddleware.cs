using System.Text.Json;
using EventManagement.Api.Models;
using EventManagement.Domain.Exceptions;

namespace EventManagement.Api.Middleware;

/// <summary>
/// Translates exceptions thrown anywhere in the request pipeline into JSON error responses.
/// The catch order is significant: the two not-found types are checked first so they produce
/// 404, then the generic <see cref="DomainException"/> base handles every other business rule
/// violation as 422, and finally anything else becomes a logged 500. DataAnnotations failures
/// short-circuit *before* this middleware (via <c>[ApiController]</c>'s automatic 400) and so
/// never reach here.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    /// <summary>Standard middleware constructor used by the ASP.NET Core pipeline.</summary>
    /// <param name="next">Next middleware in the pipeline.</param>
    /// <param name="logger">Logger used for warnings (422) and errors (500).</param>
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>Pipeline entry point. Runs the rest of the pipeline and catches anything thrown.</summary>
    /// <param name="context">The active HTTP context.</param>
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
