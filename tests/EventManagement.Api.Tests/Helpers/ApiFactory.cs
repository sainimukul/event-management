using EventManagement.Application.Interfaces;
using EventManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EventManagement.Api.Tests.Helpers;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public FixedClock Clock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            // Reset repos and clock per factory instance so tests don't share state.
            services.RemoveAll<IEventRepository>();
            services.RemoveAll<IRegistrationRepository>();
            services.RemoveAll<IClock>();
            services.AddSingleton<IEventRepository, InMemoryEventRepository>();
            services.AddSingleton<IRegistrationRepository, InMemoryRegistrationRepository>();
            services.AddSingleton<IClock>(Clock);
        });
    }
}
