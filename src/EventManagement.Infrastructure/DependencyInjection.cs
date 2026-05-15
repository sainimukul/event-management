using EventManagement.Application.Events;
using EventManagement.Application.Interfaces;
using EventManagement.Application.Registrations;
using EventManagement.Domain.Services;
using EventManagement.Infrastructure.Persistence;
using EventManagement.Infrastructure.Time;
using Microsoft.Extensions.DependencyInjection;

namespace EventManagement.Infrastructure;

public static class DependencyInjection
{
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
