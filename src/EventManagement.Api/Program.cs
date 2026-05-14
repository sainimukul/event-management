using EventManagement.Api.Middleware;
using EventManagement.Infrastructure;
using Microsoft.AspNetCore.HttpLogging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Event Management API", Version = "v1" });
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

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(CorsPolicy);
}

app.MapControllers();

app.Run();

public partial class Program { }
