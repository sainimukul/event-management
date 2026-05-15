using EventManagement.Api.Middleware;
using EventManagement.Infrastructure;
using Microsoft.AspNetCore.HttpLogging;

// Composition root for the Event Management API.
// Order in the request pipeline is meaningful: HTTP logging first so every request appears in
// the log, then the exception middleware so any exception thrown downstream is converted to a
// JSON error before reaching the user. Swagger, CORS, and CORS-dependent features run only in
// Development to keep the production surface small.

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Event Management API",
        Version = "v1",
        Description = "REST API for managing events and attendee registrations."
    });

    // Pick up every XML doc file produced by the projects in this solution so /// comments on
    // controllers, DTOs, and request models surface in the Swagger UI.
    var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly);
    foreach (var xml in xmlFiles)
    {
        c.IncludeXmlComments(xml);
    }
});
builder.Services.AddEventManagement();

builder.Services.AddHttpLogging(o =>
{
    o.LoggingFields = HttpLoggingFields.RequestMethod
                    | HttpLoggingFields.RequestPath
                    | HttpLoggingFields.ResponseStatusCode
                    | HttpLoggingFields.Duration;
});

const string CorsPolicy = "AllowLocalDev";
builder.Services.AddCors(o => o.AddPolicy(CorsPolicy, p =>
    p.WithOrigins("http://localhost:5173")
     .AllowAnyHeader()
     .AllowAnyMethod()));

var app = builder.Build();

app.UseHttpLogging();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(CorsPolicy);
}

app.MapControllers();

app.Run();

/// <summary>
/// Marker partial declaration so <c>WebApplicationFactory&lt;Program&gt;</c> in the
/// <c>EventManagement.Api.Tests</c> project can target the host. Required because the top-level
/// statements above otherwise produce an internal <c>Program</c> class.
/// </summary>
public partial class Program { }
