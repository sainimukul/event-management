using EventManagement.Application.Events;
using EventManagement.Application.Interfaces;
using EventManagement.Application.Registrations;
using EventManagement.Domain.Services;
using EventManagement.Infrastructure.Persistence;
using EventManagement.Infrastructure.Time;
using Microsoft.Extensions.DependencyInjection;

namespace EventManagement.Infrastructure;

/// <summary>
/// Composition root for the Infrastructure layer. <see cref="AddEventManagement"/> is the
/// single extension method <c>Program.cs</c> calls — it wires every service the API needs
/// so the host project doesn't have to know which concrete type implements which abstraction.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers Application services and Infrastructure implementations into the container.
    /// All registrations are singletons because the in-memory store *is* the database — losing
    /// it on scope disposal would discard every event and registration in the process.
    /// </summary>
    /// <param name="services">The DI container being configured.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for fluent chaining.</returns>
    public static IServiceCollection AddEventManagement(this IServiceCollection services)
    {
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IEventRepository, InMemoryEventRepository>();
        services.AddSingleton<IRegistrationRepository, InMemoryRegistrationRepository>();
        services.AddSingleton<RegistrationRules>();
        services.AddSingleton<EventService>();
        services.AddSingleton<RegistrationService>();
        return services;
    }
}
