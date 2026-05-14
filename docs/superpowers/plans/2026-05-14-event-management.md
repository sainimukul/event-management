# Event Management Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a take-home submission consisting of an ASP.NET Core REST API and a React + Vite SPA for managing events and attendee registrations, with unit tests for business rules and API integration tests.

**Architecture:** Backend uses a 4-project split (`Api`, `Application`, `Domain`, `Infrastructure`) with in-memory repositories, an injected `IClock`, and a per-event `SemaphoreSlim` guarding the registration critical section. Frontend uses React 18 + Vite + TypeScript with TanStack Query for server state and plain CSS Modules for styling.

**Tech Stack:** .NET 8, ASP.NET Core, xUnit + FluentAssertions + NSubstitute, `Microsoft.Extensions.Logging`, DataAnnotations, Swashbuckle, React 18, Vite, TypeScript, react-router-dom v6, TanStack Query, axios, react-hot-toast.

**Source spec:** `docs/superpowers/specs/2026-05-14-event-management-design.md` (commit `f70906f`).

**Time budget:** ~4 hours. Commits are scoped per task; each task should take 10-20 minutes.

---

## Conventions used in this plan

- File paths use forward slashes; on Windows PowerShell, replace with backslashes when running `cd`.
- Run all `dotnet` commands from the **repo root** unless stated otherwise.
- Each task ends with a single commit. Commit messages use Conventional Commits.
- TDD is applied to domain rules and services (the bits v1 explicitly asks tests for). Infrastructure and wiring tasks have lighter test discipline.

---

## Task 1: Create solution and projects

**Files:**
- Create: `EventManagement.sln`
- Create: `src/EventManagement.Api/EventManagement.Api.csproj`
- Create: `src/EventManagement.Application/EventManagement.Application.csproj`
- Create: `src/EventManagement.Domain/EventManagement.Domain.csproj`
- Create: `src/EventManagement.Infrastructure/EventManagement.Infrastructure.csproj`
- Create: `tests/EventManagement.Domain.Tests/EventManagement.Domain.Tests.csproj`
- Create: `tests/EventManagement.Application.Tests/EventManagement.Application.Tests.csproj`
- Create: `tests/EventManagement.Api.Tests/EventManagement.Api.Tests.csproj`
- Modify: `.gitignore` (add frontend ignores)

- [ ] **Step 1: Scaffold solution and source projects**

Run from repo root:

```powershell
dotnet new sln -n EventManagement
dotnet new webapi   -f net8.0 -o src/EventManagement.Api            --no-openapi --use-controllers
dotnet new classlib -f net8.0 -o src/EventManagement.Application
dotnet new classlib -f net8.0 -o src/EventManagement.Domain
dotnet new classlib -f net8.0 -o src/EventManagement.Infrastructure
```

Delete the auto-generated `Class1.cs` files from the three classlibs:

```powershell
Remove-Item src/EventManagement.Application/Class1.cs
Remove-Item src/EventManagement.Domain/Class1.cs
Remove-Item src/EventManagement.Infrastructure/Class1.cs
```

Delete the default `WeatherForecast.cs` and `Controllers/WeatherForecastController.cs` from the API project:

```powershell
Remove-Item src/EventManagement.Api/WeatherForecast.cs -ErrorAction SilentlyContinue
Remove-Item src/EventManagement.Api/Controllers/WeatherForecastController.cs -ErrorAction SilentlyContinue
```

- [ ] **Step 2: Scaffold test projects**

```powershell
dotnet new xunit -f net8.0 -o tests/EventManagement.Domain.Tests
dotnet new xunit -f net8.0 -o tests/EventManagement.Application.Tests
dotnet new xunit -f net8.0 -o tests/EventManagement.Api.Tests
Remove-Item tests/EventManagement.Domain.Tests/UnitTest1.cs
Remove-Item tests/EventManagement.Application.Tests/UnitTest1.cs
Remove-Item tests/EventManagement.Api.Tests/UnitTest1.cs
```

- [ ] **Step 3: Wire up project references**

```powershell
dotnet sln add src/EventManagement.Api/EventManagement.Api.csproj
dotnet sln add src/EventManagement.Application/EventManagement.Application.csproj
dotnet sln add src/EventManagement.Domain/EventManagement.Domain.csproj
dotnet sln add src/EventManagement.Infrastructure/EventManagement.Infrastructure.csproj
dotnet sln add tests/EventManagement.Domain.Tests/EventManagement.Domain.Tests.csproj
dotnet sln add tests/EventManagement.Application.Tests/EventManagement.Application.Tests.csproj
dotnet sln add tests/EventManagement.Api.Tests/EventManagement.Api.Tests.csproj

dotnet add src/EventManagement.Application/EventManagement.Application.csproj reference src/EventManagement.Domain/EventManagement.Domain.csproj
dotnet add src/EventManagement.Infrastructure/EventManagement.Infrastructure.csproj reference src/EventManagement.Application/EventManagement.Application.csproj
dotnet add src/EventManagement.Infrastructure/EventManagement.Infrastructure.csproj reference src/EventManagement.Domain/EventManagement.Domain.csproj
dotnet add src/EventManagement.Api/EventManagement.Api.csproj reference src/EventManagement.Application/EventManagement.Application.csproj
dotnet add src/EventManagement.Api/EventManagement.Api.csproj reference src/EventManagement.Infrastructure/EventManagement.Infrastructure.csproj

dotnet add tests/EventManagement.Domain.Tests/EventManagement.Domain.Tests.csproj reference src/EventManagement.Domain/EventManagement.Domain.csproj
dotnet add tests/EventManagement.Application.Tests/EventManagement.Application.Tests.csproj reference src/EventManagement.Application/EventManagement.Application.csproj
dotnet add tests/EventManagement.Application.Tests/EventManagement.Application.Tests.csproj reference src/EventManagement.Domain/EventManagement.Domain.csproj
dotnet add tests/EventManagement.Api.Tests/EventManagement.Api.Tests.csproj reference src/EventManagement.Api/EventManagement.Api.csproj
```

- [ ] **Step 4: Add test packages**

```powershell
dotnet add tests/EventManagement.Domain.Tests/EventManagement.Domain.Tests.csproj package FluentAssertions
dotnet add tests/EventManagement.Application.Tests/EventManagement.Application.Tests.csproj package FluentAssertions
dotnet add tests/EventManagement.Application.Tests/EventManagement.Application.Tests.csproj package NSubstitute
dotnet add tests/EventManagement.Api.Tests/EventManagement.Api.Tests.csproj package FluentAssertions
dotnet add tests/EventManagement.Api.Tests/EventManagement.Api.Tests.csproj package Microsoft.AspNetCore.Mvc.Testing
```

- [ ] **Step 5: Add Swashbuckle to the API project**

```powershell
dotnet add src/EventManagement.Api/EventManagement.Api.csproj package Swashbuckle.AspNetCore
```

- [ ] **Step 6: Append frontend ignores to `.gitignore`**

Append to the existing `.gitignore`:

```gitignore

# Node / Vite (frontend)
node_modules/
src/EventManagement.Web/dist/
src/EventManagement.Web/.vite/
*.local

# IDE
.vs/
.idea/
.vscode/
```

- [ ] **Step 7: Verify the solution builds**

```powershell
dotnet build
```

Expected: build succeeds with 0 errors. Warnings about no code in classlibs are fine.

- [ ] **Step 8: Commit**

```powershell
git add EventManagement.sln src tests .gitignore
git commit -m "chore: scaffold .NET 8 solution and test projects"
```

---

## Task 2: Domain entities, exceptions, and IClock

**Files:**
- Create: `src/EventManagement.Domain/Entities/Event.cs`
- Create: `src/EventManagement.Domain/Entities/Registration.cs`
- Create: `src/EventManagement.Domain/Exceptions/DomainException.cs`
- Create: `src/EventManagement.Domain/Exceptions/EventInPastException.cs`
- Create: `src/EventManagement.Domain/Exceptions/EventCapacityExceededException.cs`
- Create: `src/EventManagement.Domain/Exceptions/DuplicateRegistrationException.cs`
- Create: `src/EventManagement.Domain/Exceptions/EventNotFoundException.cs`
- Create: `src/EventManagement.Domain/Exceptions/RegistrationNotFoundException.cs`
- Create: `src/EventManagement.Application/Interfaces/IClock.cs`

- [ ] **Step 1: Create `Event.cs`**

```csharp
namespace EventManagement.Domain.Entities;

public sealed class Event
{
    public Guid Id { get; init; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset Date { get; set; }
    public int MaxCapacity { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
}
```

- [ ] **Step 2: Create `Registration.cs`**

```csharp
namespace EventManagement.Domain.Entities;

public sealed class Registration
{
    public Guid Id { get; init; }
    public Guid EventId { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public DateTimeOffset RegisteredAt { get; init; }
}
```

- [ ] **Step 3: Create `DomainException.cs` base class**

```csharp
namespace EventManagement.Domain.Exceptions;

public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}
```

- [ ] **Step 4: Create exception subclasses**

`EventInPastException.cs`:
```csharp
namespace EventManagement.Domain.Exceptions;

public sealed class EventInPastException : DomainException
{
    public EventInPastException()
        : base("Cannot register for an event that has already occurred.") { }
}
```

`EventCapacityExceededException.cs`:
```csharp
namespace EventManagement.Domain.Exceptions;

public sealed class EventCapacityExceededException : DomainException
{
    public EventCapacityExceededException()
        : base("Event has reached its maximum capacity.") { }
}
```

`DuplicateRegistrationException.cs`:
```csharp
namespace EventManagement.Domain.Exceptions;

public sealed class DuplicateRegistrationException : DomainException
{
    public DuplicateRegistrationException()
        : base("User is already registered for this event.") { }
}
```

`EventNotFoundException.cs`:
```csharp
namespace EventManagement.Domain.Exceptions;

public sealed class EventNotFoundException : DomainException
{
    public EventNotFoundException(Guid eventId)
        : base($"Event {eventId} was not found.") { }
}
```

`RegistrationNotFoundException.cs`:
```csharp
namespace EventManagement.Domain.Exceptions;

public sealed class RegistrationNotFoundException : DomainException
{
    public RegistrationNotFoundException(Guid registrationId)
        : base($"Registration {registrationId} was not found.") { }
}
```

- [ ] **Step 5: Create `IClock.cs` in Application/Interfaces**

```csharp
namespace EventManagement.Application.Interfaces;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
```

- [ ] **Step 6: Build**

```powershell
dotnet build
```

Expected: build succeeds.

- [ ] **Step 7: Commit**

```powershell
git add src/EventManagement.Domain src/EventManagement.Application/Interfaces/IClock.cs
git commit -m "feat(domain): add Event/Registration entities, exceptions, and IClock"
```

---

## Task 3: TDD `RegistrationRules` (all three rules)

**Files:**
- Create: `tests/EventManagement.Domain.Tests/RegistrationRulesTests.cs`
- Create: `src/EventManagement.Domain/Services/RegistrationRules.cs`

- [ ] **Step 1: Write failing tests**

`tests/EventManagement.Domain.Tests/RegistrationRulesTests.cs`:

```csharp
using EventManagement.Domain.Entities;
using EventManagement.Domain.Exceptions;
using EventManagement.Domain.Services;
using FluentAssertions;

namespace EventManagement.Domain.Tests;

public class RegistrationRulesTests
{
    private readonly DateTimeOffset _now = new(2026, 5, 14, 12, 0, 0, TimeSpan.Zero);
    private readonly RegistrationRules _rules = new();

    private Event MakeEvent(DateTimeOffset date, int capacity = 10) => new()
    {
        Id = Guid.NewGuid(),
        Title = "Test",
        Date = date,
        MaxCapacity = capacity,
        CreatedAt = _now,
        UpdatedAt = _now,
    };

    [Fact]
    public void All_rules_pass_for_future_event_with_capacity_and_new_user()
    {
        var ev = MakeEvent(_now.AddDays(1));
        var act = () =>
        {
            _rules.EnsureEventIsNotInPast(ev, _now);
            _rules.EnsureCapacityAvailable(ev, currentRegistrationCount: 5);
            _rules.EnsureUserNotAlreadyRegistered("user-1", Array.Empty<Registration>());
        };

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureEventIsNotInPast_throws_when_event_date_is_before_now()
    {
        var ev = MakeEvent(_now.AddMinutes(-1));
        var act = () => _rules.EnsureEventIsNotInPast(ev, _now);

        act.Should().Throw<EventInPastException>();
    }

    [Fact]
    public void EnsureCapacityAvailable_throws_when_current_count_equals_max_capacity()
    {
        var ev = MakeEvent(_now.AddDays(1), capacity: 3);
        var act = () => _rules.EnsureCapacityAvailable(ev, currentRegistrationCount: 3);

        act.Should().Throw<EventCapacityExceededException>();
    }

    [Fact]
    public void EnsureUserNotAlreadyRegistered_throws_when_user_has_existing_registration()
    {
        var existing = new Registration
        {
            Id = Guid.NewGuid(),
            EventId = Guid.NewGuid(),
            UserId = "user-1",
            UserName = "Alice",
            RegisteredAt = _now,
        };

        var act = () => _rules.EnsureUserNotAlreadyRegistered("user-1", new[] { existing });

        act.Should().Throw<DuplicateRegistrationException>();
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

```powershell
dotnet test tests/EventManagement.Domain.Tests
```

Expected: compile error — `RegistrationRules` does not exist.

- [ ] **Step 3: Implement `RegistrationRules`**

`src/EventManagement.Domain/Services/RegistrationRules.cs`:

```csharp
using EventManagement.Domain.Entities;
using EventManagement.Domain.Exceptions;

namespace EventManagement.Domain.Services;

public sealed class RegistrationRules
{
    public void EnsureEventIsNotInPast(Event ev, DateTimeOffset now)
    {
        if (ev.Date < now)
            throw new EventInPastException();
    }

    public void EnsureCapacityAvailable(Event ev, int currentRegistrationCount)
    {
        if (currentRegistrationCount >= ev.MaxCapacity)
            throw new EventCapacityExceededException();
    }

    public void EnsureUserNotAlreadyRegistered(string userId, IEnumerable<Registration> registrations)
    {
        if (registrations.Any(r => r.UserId == userId))
            throw new DuplicateRegistrationException();
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

```powershell
dotnet test tests/EventManagement.Domain.Tests
```

Expected: 4 tests pass.

- [ ] **Step 5: Commit**

```powershell
git add src/EventManagement.Domain/Services tests/EventManagement.Domain.Tests
git commit -m "feat(domain): add RegistrationRules with tests for all three business rules"
```

---

## Task 4: Application interfaces, DTOs, and request inputs

**Files:**
- Create: `src/EventManagement.Application/Interfaces/IEventRepository.cs`
- Create: `src/EventManagement.Application/Interfaces/IRegistrationRepository.cs`
- Create: `src/EventManagement.Application/Events/DTOs/EventDto.cs`
- Create: `src/EventManagement.Application/Events/DTOs/EventDetailDto.cs`
- Create: `src/EventManagement.Application/Registrations/DTOs/RegistrationDto.cs`
- Create: `src/EventManagement.Application/Events/Requests/CreateEventInput.cs`
- Create: `src/EventManagement.Application/Events/Requests/UpdateEventInput.cs`
- Create: `src/EventManagement.Application/Registrations/Requests/RegisterUserInput.cs`

- [ ] **Step 1: Create `IEventRepository.cs`**

```csharp
using EventManagement.Domain.Entities;

namespace EventManagement.Application.Interfaces;

public interface IEventRepository
{
    Task<IReadOnlyList<Event>> GetAllAsync();
    Task<Event?> GetByIdAsync(Guid id);
    Task AddAsync(Event ev);
    Task UpdateAsync(Event ev);
}
```

- [ ] **Step 2: Create `IRegistrationRepository.cs`**

```csharp
using EventManagement.Domain.Entities;

namespace EventManagement.Application.Interfaces;

public interface IRegistrationRepository
{
    Task<IReadOnlyList<Registration>> GetByEventIdAsync(Guid eventId);
    Task<Registration?> GetByIdAsync(Guid id);
    Task AddAsync(Registration registration);
    Task DeleteAsync(Guid registrationId);
    Task<bool> ExistsAsync(Guid eventId, string userId);
    Task<int> CountByEventIdAsync(Guid eventId);
}
```

- [ ] **Step 3: Create DTOs**

`Events/DTOs/EventDto.cs`:
```csharp
namespace EventManagement.Application.Events.DTOs;

public sealed record EventDto(
    Guid Id,
    string Title,
    DateTimeOffset Date,
    int MaxCapacity,
    int CurrentRegistrations,
    DateTimeOffset CreatedAt);
```

`Events/DTOs/EventDetailDto.cs`:
```csharp
namespace EventManagement.Application.Events.DTOs;

public sealed record EventDetailDto(
    Guid Id,
    string Title,
    string? Description,
    DateTimeOffset Date,
    int MaxCapacity,
    int CurrentRegistrations,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
```

`Registrations/DTOs/RegistrationDto.cs`:
```csharp
namespace EventManagement.Application.Registrations.DTOs;

public sealed record RegistrationDto(
    Guid Id,
    Guid EventId,
    string UserId,
    string UserName,
    DateTimeOffset RegisteredAt);
```

- [ ] **Step 4: Create application-layer input records**

`Events/Requests/CreateEventInput.cs`:
```csharp
namespace EventManagement.Application.Events.Requests;

public sealed record CreateEventInput(string Title, string? Description, DateTimeOffset Date, int MaxCapacity);
```

`Events/Requests/UpdateEventInput.cs`:
```csharp
namespace EventManagement.Application.Events.Requests;

public sealed record UpdateEventInput(string Title, string? Description, DateTimeOffset Date, int MaxCapacity);
```

`Registrations/Requests/RegisterUserInput.cs`:
```csharp
namespace EventManagement.Application.Registrations.Requests;

public sealed record RegisterUserInput(string UserId, string UserName);
```

- [ ] **Step 5: Build**

```powershell
dotnet build
```

Expected: build succeeds.

- [ ] **Step 6: Commit**

```powershell
git add src/EventManagement.Application
git commit -m "feat(application): add repository interfaces, DTOs, and request inputs"
```

---

## Task 5: TDD `EventService`

**Files:**
- Create: `tests/EventManagement.Application.Tests/Events/EventServiceTests.cs`
- Create: `src/EventManagement.Application/Events/EventService.cs`

- [ ] **Step 1: Write failing tests**

`tests/EventManagement.Application.Tests/Events/EventServiceTests.cs`:

```csharp
using EventManagement.Application.Events;
using EventManagement.Application.Events.Requests;
using EventManagement.Application.Interfaces;
using EventManagement.Domain.Entities;
using EventManagement.Domain.Exceptions;
using FluentAssertions;
using NSubstitute;

namespace EventManagement.Application.Tests.Events;

public class EventServiceTests
{
    private readonly IEventRepository _events = Substitute.For<IEventRepository>();
    private readonly IRegistrationRepository _registrations = Substitute.For<IRegistrationRepository>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly EventService _sut;

    public EventServiceTests()
    {
        _clock.UtcNow.Returns(new DateTimeOffset(2026, 5, 14, 12, 0, 0, TimeSpan.Zero));
        _sut = new EventService(_events, _registrations, _clock);
    }

    [Fact]
    public async Task CreateAsync_persists_event_and_returns_dto_with_generated_id_and_timestamps()
    {
        var input = new CreateEventInput("Conf", "Desc", new DateTimeOffset(2026, 6, 1, 9, 0, 0, TimeSpan.Zero), 50);

        var result = await _sut.CreateAsync(input);

        result.Id.Should().NotBeEmpty();
        result.Title.Should().Be("Conf");
        result.MaxCapacity.Should().Be(50);
        result.CurrentRegistrations.Should().Be(0);
        result.CreatedAt.Should().Be(_clock.UtcNow);
        await _events.Received(1).AddAsync(Arg.Is<Event>(e => e.Title == "Conf" && e.MaxCapacity == 50));
    }

    [Fact]
    public async Task UpdateAsync_throws_when_event_does_not_exist()
    {
        _events.GetByIdAsync(Arg.Any<Guid>()).Returns((Event?)null);
        var input = new UpdateEventInput("New", null, new DateTimeOffset(2026, 7, 1, 9, 0, 0, TimeSpan.Zero), 20);

        var act = () => _sut.UpdateAsync(Guid.NewGuid(), input);

        await act.Should().ThrowAsync<EventNotFoundException>();
    }

    [Fact]
    public async Task GetByIdAsync_returns_detail_dto_with_current_registration_count()
    {
        var id = Guid.NewGuid();
        var ev = new Event
        {
            Id = id,
            Title = "Conf",
            Description = "Desc",
            Date = new DateTimeOffset(2026, 6, 1, 9, 0, 0, TimeSpan.Zero),
            MaxCapacity = 50,
            CreatedAt = _clock.UtcNow,
            UpdatedAt = _clock.UtcNow,
        };
        _events.GetByIdAsync(id).Returns(ev);
        _registrations.CountByEventIdAsync(id).Returns(7);

        var result = await _sut.GetByIdAsync(id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.CurrentRegistrations.Should().Be(7);
        result.Description.Should().Be("Desc");
    }
}
```

- [ ] **Step 2: Implement `EventService`**

`src/EventManagement.Application/Events/EventService.cs`:

```csharp
using EventManagement.Application.Events.DTOs;
using EventManagement.Application.Events.Requests;
using EventManagement.Application.Interfaces;
using EventManagement.Domain.Entities;
using EventManagement.Domain.Exceptions;

namespace EventManagement.Application.Events;

public sealed class EventService
{
    private readonly IEventRepository _events;
    private readonly IRegistrationRepository _registrations;
    private readonly IClock _clock;

    public EventService(IEventRepository events, IRegistrationRepository registrations, IClock clock)
    {
        _events = events;
        _registrations = registrations;
        _clock = clock;
    }

    public async Task<EventDto> CreateAsync(CreateEventInput input)
    {
        var now = _clock.UtcNow;
        var ev = new Event
        {
            Id = Guid.NewGuid(),
            Title = input.Title.Trim(),
            Description = input.Description,
            Date = input.Date.ToUniversalTime(),
            MaxCapacity = input.MaxCapacity,
            CreatedAt = now,
            UpdatedAt = now,
        };
        await _events.AddAsync(ev);
        return new EventDto(ev.Id, ev.Title, ev.Date, ev.MaxCapacity, 0, ev.CreatedAt);
    }

    public async Task<IReadOnlyList<EventDto>> GetAllAsync()
    {
        var events = await _events.GetAllAsync();
        var result = new List<EventDto>(events.Count);
        foreach (var ev in events)
        {
            var count = await _registrations.CountByEventIdAsync(ev.Id);
            result.Add(new EventDto(ev.Id, ev.Title, ev.Date, ev.MaxCapacity, count, ev.CreatedAt));
        }
        return result;
    }

    public async Task<EventDetailDto?> GetByIdAsync(Guid id)
    {
        var ev = await _events.GetByIdAsync(id);
        if (ev is null) return null;
        var count = await _registrations.CountByEventIdAsync(id);
        return new EventDetailDto(ev.Id, ev.Title, ev.Description, ev.Date, ev.MaxCapacity, count, ev.CreatedAt, ev.UpdatedAt);
    }

    public async Task<EventDto> UpdateAsync(Guid id, UpdateEventInput input)
    {
        var ev = await _events.GetByIdAsync(id) ?? throw new EventNotFoundException(id);
        ev.Title = input.Title.Trim();
        ev.Description = input.Description;
        ev.Date = input.Date.ToUniversalTime();
        ev.MaxCapacity = input.MaxCapacity;
        ev.UpdatedAt = _clock.UtcNow;
        await _events.UpdateAsync(ev);
        var count = await _registrations.CountByEventIdAsync(id);
        return new EventDto(ev.Id, ev.Title, ev.Date, ev.MaxCapacity, count, ev.CreatedAt);
    }
}
```

- [ ] **Step 3: Run tests to verify they pass**

```powershell
dotnet test tests/EventManagement.Application.Tests
```

Expected: 3 tests pass.

- [ ] **Step 4: Commit**

```powershell
git add src/EventManagement.Application/Events tests/EventManagement.Application.Tests/Events
git commit -m "feat(application): add EventService with tests for create/update/getById"
```

---

## Task 6: TDD `RegistrationService` (with per-event lock)

**Files:**
- Create: `tests/EventManagement.Application.Tests/Registrations/RegistrationServiceTests.cs`
- Create: `src/EventManagement.Application/Registrations/RegistrationService.cs`

- [ ] **Step 1: Write failing tests**

`tests/EventManagement.Application.Tests/Registrations/RegistrationServiceTests.cs`:

```csharp
using EventManagement.Application.Interfaces;
using EventManagement.Application.Registrations;
using EventManagement.Application.Registrations.Requests;
using EventManagement.Domain.Entities;
using EventManagement.Domain.Exceptions;
using EventManagement.Domain.Services;
using FluentAssertions;
using NSubstitute;

namespace EventManagement.Application.Tests.Registrations;

public class RegistrationServiceTests
{
    private readonly IEventRepository _events = Substitute.For<IEventRepository>();
    private readonly IRegistrationRepository _registrations = Substitute.For<IRegistrationRepository>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly RegistrationRules _rules = new();
    private readonly RegistrationService _sut;
    private readonly DateTimeOffset _now = new(2026, 5, 14, 12, 0, 0, TimeSpan.Zero);

    public RegistrationServiceTests()
    {
        _clock.UtcNow.Returns(_now);
        _sut = new RegistrationService(_events, _registrations, _rules, _clock);
    }

    private Event MakeEvent(int capacity = 10, DateTimeOffset? date = null) => new()
    {
        Id = Guid.NewGuid(),
        Title = "Conf",
        Date = date ?? _now.AddDays(1),
        MaxCapacity = capacity,
        CreatedAt = _now,
        UpdatedAt = _now,
    };

    [Fact]
    public async Task RegisterAsync_success_persists_registration_and_returns_dto()
    {
        var ev = MakeEvent();
        _events.GetByIdAsync(ev.Id).Returns(ev);
        _registrations.GetByEventIdAsync(ev.Id).Returns(Array.Empty<Registration>());

        var input = new RegisterUserInput("user-1", "Alice");
        var result = await _sut.RegisterAsync(ev.Id, input);

        result.UserId.Should().Be("user-1");
        result.UserName.Should().Be("Alice");
        result.EventId.Should().Be(ev.Id);
        result.RegisteredAt.Should().Be(_now);
        await _registrations.Received(1).AddAsync(Arg.Is<Registration>(r => r.UserId == "user-1" && r.EventId == ev.Id));
    }

    [Fact]
    public async Task RegisterAsync_throws_when_event_does_not_exist()
    {
        _events.GetByIdAsync(Arg.Any<Guid>()).Returns((Event?)null);

        var act = () => _sut.RegisterAsync(Guid.NewGuid(), new RegisterUserInput("user-1", "Alice"));

        await act.Should().ThrowAsync<EventNotFoundException>();
    }

    [Fact]
    public async Task RegisterAsync_throws_when_user_is_already_registered()
    {
        var ev = MakeEvent();
        var existing = new Registration { Id = Guid.NewGuid(), EventId = ev.Id, UserId = "user-1", UserName = "Alice", RegisteredAt = _now };
        _events.GetByIdAsync(ev.Id).Returns(ev);
        _registrations.GetByEventIdAsync(ev.Id).Returns(new[] { existing });

        var act = () => _sut.RegisterAsync(ev.Id, new RegisterUserInput("user-1", "Alice"));

        await act.Should().ThrowAsync<DuplicateRegistrationException>();
    }

    [Fact]
    public async Task RegisterAsync_throws_when_event_is_at_capacity()
    {
        var ev = MakeEvent(capacity: 1);
        var existing = new Registration { Id = Guid.NewGuid(), EventId = ev.Id, UserId = "user-2", UserName = "Bob", RegisteredAt = _now };
        _events.GetByIdAsync(ev.Id).Returns(ev);
        _registrations.GetByEventIdAsync(ev.Id).Returns(new[] { existing });

        var act = () => _sut.RegisterAsync(ev.Id, new RegisterUserInput("user-1", "Alice"));

        await act.Should().ThrowAsync<EventCapacityExceededException>();
    }

    [Fact]
    public async Task UnregisterAsync_throws_when_registration_not_found()
    {
        _registrations.GetByIdAsync(Arg.Any<Guid>()).Returns((Registration?)null);

        var act = () => _sut.UnregisterAsync(Guid.NewGuid(), Guid.NewGuid());

        await act.Should().ThrowAsync<RegistrationNotFoundException>();
    }
}
```

- [ ] **Step 2: Implement `RegistrationService`**

`src/EventManagement.Application/Registrations/RegistrationService.cs`:

```csharp
using System.Collections.Concurrent;
using EventManagement.Application.Interfaces;
using EventManagement.Application.Registrations.DTOs;
using EventManagement.Application.Registrations.Requests;
using EventManagement.Domain.Entities;
using EventManagement.Domain.Exceptions;
using EventManagement.Domain.Services;

namespace EventManagement.Application.Registrations;

public sealed class RegistrationService
{
    private readonly IEventRepository _events;
    private readonly IRegistrationRepository _registrations;
    private readonly RegistrationRules _rules;
    private readonly IClock _clock;
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _eventLocks = new();

    public RegistrationService(
        IEventRepository events,
        IRegistrationRepository registrations,
        RegistrationRules rules,
        IClock clock)
    {
        _events = events;
        _registrations = registrations;
        _rules = rules;
        _clock = clock;
    }

    public async Task<RegistrationDto> RegisterAsync(Guid eventId, RegisterUserInput input)
    {
        var ev = await _events.GetByIdAsync(eventId) ?? throw new EventNotFoundException(eventId);

        var gate = _eventLocks.GetOrAdd(eventId, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync();
        try
        {
            var existing = await _registrations.GetByEventIdAsync(eventId);
            _rules.EnsureEventIsNotInPast(ev, _clock.UtcNow);
            _rules.EnsureCapacityAvailable(ev, existing.Count);
            _rules.EnsureUserNotAlreadyRegistered(input.UserId, existing);

            var registration = new Registration
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                UserId = input.UserId,
                UserName = input.UserName,
                RegisteredAt = _clock.UtcNow,
            };
            await _registrations.AddAsync(registration);
            return ToDto(registration);
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task UnregisterAsync(Guid eventId, Guid registrationId)
    {
        var existing = await _registrations.GetByIdAsync(registrationId)
            ?? throw new RegistrationNotFoundException(registrationId);
        if (existing.EventId != eventId)
            throw new RegistrationNotFoundException(registrationId);
        await _registrations.DeleteAsync(registrationId);
    }

    public async Task<IReadOnlyList<RegistrationDto>> ListForEventAsync(Guid eventId)
    {
        var registrations = await _registrations.GetByEventIdAsync(eventId);
        return registrations.Select(ToDto).ToList();
    }

    private static RegistrationDto ToDto(Registration r) =>
        new(r.Id, r.EventId, r.UserId, r.UserName, r.RegisteredAt);
}
```

- [ ] **Step 3: Run tests to verify they pass**

```powershell
dotnet test tests/EventManagement.Application.Tests
```

Expected: 5 new tests pass (plus 3 from EventService = 8 total).

- [ ] **Step 4: Commit**

```powershell
git add src/EventManagement.Application/Registrations tests/EventManagement.Application.Tests/Registrations
git commit -m "feat(application): add RegistrationService with per-event lock and tests"
```

---

## Task 7: Infrastructure layer (in-memory repos + SystemClock + DI)

**Files:**
- Create: `src/EventManagement.Infrastructure/Persistence/InMemoryEventRepository.cs`
- Create: `src/EventManagement.Infrastructure/Persistence/InMemoryRegistrationRepository.cs`
- Create: `src/EventManagement.Infrastructure/Time/SystemClock.cs`
- Create: `src/EventManagement.Infrastructure/DependencyInjection.cs`

- [ ] **Step 1: Add the DI extensions package**

```powershell
dotnet add src/EventManagement.Infrastructure/EventManagement.Infrastructure.csproj package Microsoft.Extensions.DependencyInjection.Abstractions
```

- [ ] **Step 2: Create `InMemoryEventRepository.cs`**

```csharp
using System.Collections.Concurrent;
using EventManagement.Application.Interfaces;
using EventManagement.Domain.Entities;

namespace EventManagement.Infrastructure.Persistence;

public sealed class InMemoryEventRepository : IEventRepository
{
    private readonly ConcurrentDictionary<Guid, Event> _store = new();

    public Task<IReadOnlyList<Event>> GetAllAsync() =>
        Task.FromResult<IReadOnlyList<Event>>(_store.Values.OrderBy(e => e.Date).ToList());

    public Task<Event?> GetByIdAsync(Guid id)
    {
        _store.TryGetValue(id, out var ev);
        return Task.FromResult(ev);
    }

    public Task AddAsync(Event ev)
    {
        _store[ev.Id] = ev;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Event ev)
    {
        _store[ev.Id] = ev;
        return Task.CompletedTask;
    }
}
```

- [ ] **Step 3: Create `InMemoryRegistrationRepository.cs`**

```csharp
using System.Collections.Concurrent;
using EventManagement.Application.Interfaces;
using EventManagement.Domain.Entities;

namespace EventManagement.Infrastructure.Persistence;

public sealed class InMemoryRegistrationRepository : IRegistrationRepository
{
    private readonly ConcurrentDictionary<Guid, Registration> _store = new();

    public Task<IReadOnlyList<Registration>> GetByEventIdAsync(Guid eventId) =>
        Task.FromResult<IReadOnlyList<Registration>>(
            _store.Values.Where(r => r.EventId == eventId).OrderBy(r => r.RegisteredAt).ToList());

    public Task<Registration?> GetByIdAsync(Guid id)
    {
        _store.TryGetValue(id, out var r);
        return Task.FromResult(r);
    }

    public Task AddAsync(Registration registration)
    {
        _store[registration.Id] = registration;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid registrationId)
    {
        _store.TryRemove(registrationId, out _);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(Guid eventId, string userId) =>
        Task.FromResult(_store.Values.Any(r => r.EventId == eventId && r.UserId == userId));

    public Task<int> CountByEventIdAsync(Guid eventId) =>
        Task.FromResult(_store.Values.Count(r => r.EventId == eventId));
}
```

- [ ] **Step 4: Create `SystemClock.cs`**

```csharp
using EventManagement.Application.Interfaces;

namespace EventManagement.Infrastructure.Time;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
```

- [ ] **Step 5: Create `DependencyInjection.cs`**

```csharp
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
```

- [ ] **Step 6: Build**

```powershell
dotnet build
```

Expected: build succeeds.

- [ ] **Step 7: Commit**

```powershell
git add src/EventManagement.Infrastructure
git commit -m "feat(infrastructure): add in-memory repos, SystemClock, and DI extension"
```

---

## Task 8: API request models + Exception middleware

**Files:**
- Create: `src/EventManagement.Api/Models/Events/CreateEventRequest.cs`
- Create: `src/EventManagement.Api/Models/Events/UpdateEventRequest.cs`
- Create: `src/EventManagement.Api/Models/Registrations/RegisterUserRequest.cs`
- Create: `src/EventManagement.Api/Middleware/ExceptionHandlingMiddleware.cs`
- Create: `src/EventManagement.Api/Models/ErrorResponse.cs`

- [ ] **Step 1: Create `CreateEventRequest.cs`**

```csharp
using System.ComponentModel.DataAnnotations;

namespace EventManagement.Api.Models.Events;

public sealed class CreateEventRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required]
    public DateTimeOffset? Date { get; set; }

    [Range(1, int.MaxValue)]
    public int MaxCapacity { get; set; }
}
```

- [ ] **Step 2: Create `UpdateEventRequest.cs`**

```csharp
using System.ComponentModel.DataAnnotations;

namespace EventManagement.Api.Models.Events;

public sealed class UpdateEventRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required]
    public DateTimeOffset? Date { get; set; }

    [Range(1, int.MaxValue)]
    public int MaxCapacity { get; set; }
}
```

- [ ] **Step 3: Create `RegisterUserRequest.cs`**

```csharp
using System.ComponentModel.DataAnnotations;

namespace EventManagement.Api.Models.Registrations;

public sealed class RegisterUserRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string UserName { get; set; } = string.Empty;
}
```

- [ ] **Step 4: Create `ErrorResponse.cs`**

```csharp
namespace EventManagement.Api.Models;

public sealed record ErrorResponse(string Message, int StatusCode, string? Details = null);
```

- [ ] **Step 5: Create `ExceptionHandlingMiddleware.cs`**

```csharp
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
```

- [ ] **Step 6: Build**

```powershell
dotnet build
```

Expected: build succeeds.

- [ ] **Step 7: Commit**

```powershell
git add src/EventManagement.Api/Models src/EventManagement.Api/Middleware
git commit -m "feat(api): add request models with DataAnnotations and exception middleware"
```

---

## Task 9: API controllers

**Files:**
- Create: `src/EventManagement.Api/Controllers/EventsController.cs`
- Create: `src/EventManagement.Api/Controllers/RegistrationsController.cs`

- [ ] **Step 1: Create `EventsController.cs`**

```csharp
using EventManagement.Api.Models.Events;
using EventManagement.Application.Events;
using EventManagement.Application.Events.Requests;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.Api.Controllers;

[ApiController]
[Route("api/events")]
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
```

- [ ] **Step 2: Create `RegistrationsController.cs`**

```csharp
using EventManagement.Api.Models.Registrations;
using EventManagement.Application.Registrations;
using EventManagement.Application.Registrations.Requests;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.Api.Controllers;

[ApiController]
[Route("api/events/{eventId:guid}/registrations")]
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
        return CreatedAtAction(nameof(List), new { eventId }, created);
    }

    [HttpDelete("{registrationId:guid}")]
    public async Task<IActionResult> Unregister(Guid eventId, Guid registrationId)
    {
        await _registrations.UnregisterAsync(eventId, registrationId);
        return NoContent();
    }
}
```

- [ ] **Step 3: Build**

```powershell
dotnet build
```

Expected: build succeeds.

- [ ] **Step 4: Commit**

```powershell
git add src/EventManagement.Api/Controllers
git commit -m "feat(api): add EventsController and RegistrationsController"
```

---

## Task 10: Program.cs wiring + appsettings + launchSettings

**Files:**
- Modify: `src/EventManagement.Api/Program.cs` (replace contents)
- Modify: `src/EventManagement.Api/appsettings.json` (replace contents)
- Modify: `src/EventManagement.Api/appsettings.Development.json` (replace contents)
- Modify: `src/EventManagement.Api/Properties/launchSettings.json` (replace contents)

- [ ] **Step 1: Replace `Program.cs`**

```csharp
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
```

- [ ] **Step 2: Replace `appsettings.json`**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

- [ ] **Step 3: Replace `appsettings.Development.json`**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Information"
    }
  }
}
```

- [ ] **Step 4: Replace `Properties/launchSettings.json`**

```json
{
  "$schema": "http://json.schemastore.org/launchsettings.json",
  "profiles": {
    "http": {
      "commandName": "Project",
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "http://localhost:5050",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

- [ ] **Step 5: Run the API and smoke-test Swagger**

```powershell
dotnet run --project src/EventManagement.Api
```

Open `http://localhost:5050/swagger` in a browser. Expected: Swagger UI lists `/api/events` and `/api/events/{eventId}/registrations` endpoints. Stop the server with Ctrl+C.

- [ ] **Step 6: Commit**

```powershell
git add src/EventManagement.Api/Program.cs src/EventManagement.Api/appsettings*.json src/EventManagement.Api/Properties/launchSettings.json
git commit -m "feat(api): wire DI, Swagger, HTTP logging, CORS, exception middleware"
```

---

## Task 11: API integration tests

**Files:**
- Create: `tests/EventManagement.Api.Tests/Helpers/ApiFactory.cs`
- Create: `tests/EventManagement.Api.Tests/Helpers/FixedClock.cs`
- Create: `tests/EventManagement.Api.Tests/EventsControllerTests.cs`
- Create: `tests/EventManagement.Api.Tests/RegistrationsControllerTests.cs`

- [ ] **Step 1: Create `FixedClock.cs`**

```csharp
using EventManagement.Application.Interfaces;

namespace EventManagement.Api.Tests.Helpers;

public sealed class FixedClock : IClock
{
    public DateTimeOffset UtcNow { get; set; } = new(2026, 5, 14, 12, 0, 0, TimeSpan.Zero);
}
```

- [ ] **Step 2: Create `ApiFactory.cs`**

```csharp
using EventManagement.Application.Interfaces;
using EventManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EventManagement.Api.Tests.Helpers;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public FixedClock Clock { get; } = new();

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
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
```

- [ ] **Step 3: Create `EventsControllerTests.cs`**

```csharp
using System.Net;
using System.Net.Http.Json;
using EventManagement.Api.Tests.Helpers;
using EventManagement.Application.Events.DTOs;
using FluentAssertions;

namespace EventManagement.Api.Tests;

public class EventsControllerTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    public EventsControllerTests(ApiFactory factory) => _factory = factory;

    private object ValidCreateBody() => new
    {
        title = "Conf 2026",
        description = "Annual",
        date = new DateTimeOffset(2026, 8, 1, 9, 0, 0, TimeSpan.Zero),
        maxCapacity = 50,
    };

    [Fact]
    public async Task POST_events_with_valid_body_returns_201_with_location_header()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/events", ValidCreateBody());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        var dto = await response.Content.ReadFromJsonAsync<EventDto>();
        dto!.Title.Should().Be("Conf 2026");
    }

    [Fact]
    public async Task GET_events_unknown_id_returns_404()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/events/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task POST_events_with_empty_title_returns_400()
    {
        var client = _factory.CreateClient();
        var body = new
        {
            title = "",
            description = (string?)null,
            date = new DateTimeOffset(2026, 8, 1, 9, 0, 0, TimeSpan.Zero),
            maxCapacity = 10,
        };

        var response = await client.PostAsJsonAsync("/api/events", body);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
```

- [ ] **Step 4: Create `RegistrationsControllerTests.cs`**

```csharp
using System.Net;
using System.Net.Http.Json;
using EventManagement.Api.Tests.Helpers;
using EventManagement.Application.Events.DTOs;
using EventManagement.Application.Registrations.DTOs;
using FluentAssertions;

namespace EventManagement.Api.Tests;

public class RegistrationsControllerTests
{
    private static object CreateEventBody(DateTimeOffset date, int capacity = 10) => new
    {
        title = "Conf",
        description = (string?)null,
        date,
        maxCapacity = capacity,
    };

    private static object RegisterBody(string userId, string userName) => new { userId, userName };

    private async Task<Guid> CreateEvent(HttpClient client, DateTimeOffset date, int capacity = 10)
    {
        var response = await client.PostAsJsonAsync("/api/events", CreateEventBody(date, capacity));
        response.EnsureSuccessStatusCode();
        var dto = await response.Content.ReadFromJsonAsync<EventDto>();
        return dto!.Id;
    }

    [Fact]
    public async Task POST_registration_for_future_event_returns_201()
    {
        await using var factory = new ApiFactory();
        var client = factory.CreateClient();
        var eventId = await CreateEvent(client, factory.Clock.UtcNow.AddDays(1));

        var response = await client.PostAsJsonAsync($"/api/events/{eventId}/registrations",
            RegisterBody("user-1", "Alice"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var dto = await response.Content.ReadFromJsonAsync<RegistrationDto>();
        dto!.UserId.Should().Be("user-1");
    }

    [Fact]
    public async Task POST_duplicate_registration_returns_422()
    {
        await using var factory = new ApiFactory();
        var client = factory.CreateClient();
        var eventId = await CreateEvent(client, factory.Clock.UtcNow.AddDays(1));
        await client.PostAsJsonAsync($"/api/events/{eventId}/registrations", RegisterBody("user-1", "Alice"));

        var response = await client.PostAsJsonAsync($"/api/events/{eventId}/registrations",
            RegisterBody("user-1", "Alice"));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task POST_registration_when_at_capacity_returns_422()
    {
        await using var factory = new ApiFactory();
        var client = factory.CreateClient();
        var eventId = await CreateEvent(client, factory.Clock.UtcNow.AddDays(1), capacity: 1);
        await client.PostAsJsonAsync($"/api/events/{eventId}/registrations", RegisterBody("user-1", "Alice"));

        var response = await client.PostAsJsonAsync($"/api/events/{eventId}/registrations",
            RegisterBody("user-2", "Bob"));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task POST_registration_for_past_event_returns_422()
    {
        await using var factory = new ApiFactory();
        var client = factory.CreateClient();
        // Create event in the future, then move the clock past it.
        var eventDate = factory.Clock.UtcNow.AddHours(1);
        var eventId = await CreateEvent(client, eventDate);
        factory.Clock.UtcNow = eventDate.AddMinutes(1);

        var response = await client.PostAsJsonAsync($"/api/events/{eventId}/registrations",
            RegisterBody("user-1", "Alice"));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
```

- [ ] **Step 5: Run all backend tests**

```powershell
dotnet test
```

Expected: 4 domain + 8 application + 3 events + 4 registrations = **19 tests pass**.

- [ ] **Step 6: Commit**

```powershell
git add tests/EventManagement.Api.Tests
git commit -m "test(api): add integration tests for events and registrations endpoints"
```

---

## Task 12: Frontend scaffold

**Files:**
- Create: `src/EventManagement.Web/` (Vite scaffold)
- Modify: `src/EventManagement.Web/package.json` (add dependencies)
- Replace: `src/EventManagement.Web/src/index.css`
- Replace: `src/EventManagement.Web/src/App.tsx`
- Replace: `src/EventManagement.Web/src/main.tsx`
- Delete: `src/EventManagement.Web/src/App.css`
- Create: `src/EventManagement.Web/.env.development`

- [ ] **Step 1: Scaffold Vite app**

From repo root:

```powershell
npm create vite@latest src/EventManagement.Web -- --template react-ts
```

If prompted to install create-vite, answer yes.

- [ ] **Step 2: Install dependencies**

```powershell
cd src/EventManagement.Web
npm install
npm install @tanstack/react-query axios react-router-dom react-hot-toast
cd ../..
```

- [ ] **Step 3: Create `.env.development`**

`src/EventManagement.Web/.env.development`:

```
VITE_API_BASE_URL=http://localhost:5050
```

- [ ] **Step 4: Replace `src/index.css`**

`src/EventManagement.Web/src/index.css`:

```css
:root {
  --color-bg: #f6f7f9;
  --color-surface: #ffffff;
  --color-text: #1a1a1a;
  --color-muted: #6b7280;
  --color-primary: #2563eb;
  --color-primary-hover: #1d4ed8;
  --color-danger: #dc2626;
  --color-danger-hover: #b91c1c;
  --color-border: #e5e7eb;
  --radius: 8px;
  --shadow-sm: 0 1px 2px rgba(0, 0, 0, 0.05);
  font-family: system-ui, -apple-system, "Segoe UI", Roboto, sans-serif;
  color: var(--color-text);
  background: var(--color-bg);
}

* { box-sizing: border-box; }
body { margin: 0; min-height: 100vh; }

a { color: var(--color-primary); text-decoration: none; }
a:hover { text-decoration: underline; }

.app-shell {
  max-width: 720px;
  margin: 0 auto;
  padding: 2rem 1rem 4rem;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius);
  padding: 1rem;
  box-shadow: var(--shadow-sm);
}

.button {
  display: inline-block;
  padding: 0.5rem 1rem;
  border-radius: var(--radius);
  border: 1px solid var(--color-border);
  background: var(--color-surface);
  color: var(--color-text);
  cursor: pointer;
  font: inherit;
  text-decoration: none;
}
.button:hover { background: #f3f4f6; }
.button[disabled] { opacity: 0.6; cursor: not-allowed; }

.button--primary {
  background: var(--color-primary);
  color: #fff;
  border-color: var(--color-primary);
}
.button--primary:hover { background: var(--color-primary-hover); border-color: var(--color-primary-hover); }

.button--danger {
  background: var(--color-danger);
  color: #fff;
  border-color: var(--color-danger);
}
.button--danger:hover { background: var(--color-danger-hover); border-color: var(--color-danger-hover); }

.button--ghost { background: transparent; border-color: transparent; color: var(--color-primary); }
.button--ghost:hover { background: #eef2ff; }

.form-row { display: flex; flex-direction: column; gap: 0.35rem; margin-bottom: 1rem; }
.label { font-weight: 600; font-size: 0.9rem; }
.input, .textarea {
  font: inherit;
  padding: 0.5rem 0.75rem;
  border: 1px solid var(--color-border);
  border-radius: var(--radius);
  background: var(--color-surface);
}
.textarea { min-height: 80px; resize: vertical; }
.input:focus, .textarea:focus { outline: 2px solid var(--color-primary); outline-offset: -1px; }

.field-error { color: var(--color-danger); font-size: 0.85rem; margin-top: 0.25rem; }
.muted { color: var(--color-muted); }
```

- [ ] **Step 5: Replace `src/main.tsx`**

`src/EventManagement.Web/src/main.tsx`:

```tsx
import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { Toaster } from "react-hot-toast";
import "./index.css";
import App from "./App";

const queryClient = new QueryClient({
  defaultOptions: { queries: { retry: 1, refetchOnWindowFocus: false } },
});

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <App />
        <Toaster position="top-right" />
      </BrowserRouter>
    </QueryClientProvider>
  </StrictMode>
);
```

- [ ] **Step 6: Replace `src/App.tsx` with router skeleton**

`src/EventManagement.Web/src/App.tsx`:

```tsx
import { Routes, Route, Link } from "react-router-dom";

function Placeholder({ title }: { title: string }) {
  return <div className="card">{title} — coming soon</div>;
}

export default function App() {
  return (
    <div className="app-shell">
      <header className="page-header">
        <Link to="/" style={{ fontWeight: 700, fontSize: "1.25rem", color: "var(--color-text)" }}>
          Event Manager
        </Link>
      </header>
      <Routes>
        <Route path="/" element={<Placeholder title="Event List" />} />
        <Route path="/events/new" element={<Placeholder title="Create Event" />} />
        <Route path="/events/:id" element={<Placeholder title="Event Detail" />} />
      </Routes>
    </div>
  );
}
```

- [ ] **Step 7: Delete leftover Vite scaffold**

```powershell
Remove-Item src/EventManagement.Web/src/App.css -ErrorAction SilentlyContinue
Remove-Item src/EventManagement.Web/src/assets/react.svg -ErrorAction SilentlyContinue
```

- [ ] **Step 8: Smoke-test the dev server**

```powershell
cd src/EventManagement.Web
npm run dev
```

Open `http://localhost:5173`. Expected: page renders with the "Event Manager" header and "Event List — coming soon" card. Stop with Ctrl+C, then `cd ../..`.

- [ ] **Step 9: Commit**

```powershell
git add src/EventManagement.Web .gitignore
git commit -m "feat(web): scaffold Vite + React + TS app with router and global styles"
```

---

## Task 13: API client, types, and shared utilities

**Files:**
- Create: `src/EventManagement.Web/src/api/client.ts`
- Create: `src/EventManagement.Web/src/api/eventsApi.ts`
- Create: `src/EventManagement.Web/src/api/registrationsApi.ts`
- Create: `src/EventManagement.Web/src/features/events/types/event.ts`
- Create: `src/EventManagement.Web/src/features/registrations/types/registration.ts`
- Create: `src/EventManagement.Web/src/shared/utils/date.ts`
- Create: `src/EventManagement.Web/src/shared/utils/apiError.ts`

- [ ] **Step 1: Create `api/client.ts`**

```ts
import axios from "axios";

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5050",
  headers: { "Content-Type": "application/json" },
});
```

- [ ] **Step 2: Create event types**

`src/features/events/types/event.ts`:

```ts
export interface EventSummary {
  id: string;
  title: string;
  date: string;
  maxCapacity: number;
  currentRegistrations: number;
  createdAt: string;
}

export interface EventDetail extends EventSummary {
  description?: string | null;
  updatedAt: string;
}

export interface EventFormValues {
  title: string;
  description: string;
  date: string;
  maxCapacity: number;
}
```

- [ ] **Step 3: Create registration types**

`src/features/registrations/types/registration.ts`:

```ts
export interface Registration {
  id: string;
  eventId: string;
  userId: string;
  userName: string;
  registeredAt: string;
}

export interface RegisterUserPayload {
  userId: string;
  userName: string;
}
```

- [ ] **Step 4: Create `api/eventsApi.ts`**

```ts
import { apiClient } from "./client";
import type { EventSummary, EventDetail } from "../features/events/types/event";

export interface CreateEventPayload {
  title: string;
  description?: string | null;
  date: string;
  maxCapacity: number;
}

export const eventsApi = {
  list: async (): Promise<EventSummary[]> => {
    const { data } = await apiClient.get<EventSummary[]>("/api/events");
    return data;
  },
  get: async (id: string): Promise<EventDetail> => {
    const { data } = await apiClient.get<EventDetail>(`/api/events/${id}`);
    return data;
  },
  create: async (payload: CreateEventPayload): Promise<EventSummary> => {
    const { data } = await apiClient.post<EventSummary>("/api/events", payload);
    return data;
  },
  update: async (id: string, payload: CreateEventPayload): Promise<EventSummary> => {
    const { data } = await apiClient.put<EventSummary>(`/api/events/${id}`, payload);
    return data;
  },
};
```

- [ ] **Step 5: Create `api/registrationsApi.ts`**

```ts
import { apiClient } from "./client";
import type { Registration, RegisterUserPayload } from "../features/registrations/types/registration";

export const registrationsApi = {
  list: async (eventId: string): Promise<Registration[]> => {
    const { data } = await apiClient.get<Registration[]>(`/api/events/${eventId}/registrations`);
    return data;
  },
  register: async (eventId: string, payload: RegisterUserPayload): Promise<Registration> => {
    const { data } = await apiClient.post<Registration>(`/api/events/${eventId}/registrations`, payload);
    return data;
  },
  unregister: async (eventId: string, registrationId: string): Promise<void> => {
    await apiClient.delete(`/api/events/${eventId}/registrations/${registrationId}`);
  },
};
```

- [ ] **Step 6: Create `shared/utils/date.ts`**

```ts
// Convert an ISO 8601 string (UTC) to the value expected by <input type="datetime-local">.
export function toLocalInputValue(iso: string): string {
  const d = new Date(iso);
  const pad = (n: number) => String(n).padStart(2, "0");
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
}

// Convert the value from <input type="datetime-local"> (local time, no tz) to a UTC ISO 8601 string.
export function fromLocalInputValue(localValue: string): string {
  return new Date(localValue).toISOString();
}

export function formatDate(iso: string): string {
  return new Intl.DateTimeFormat(undefined, {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(iso));
}
```

- [ ] **Step 7: Create `shared/utils/apiError.ts`**

```ts
import axios from "axios";

export interface ApiErrorInfo {
  message: string;
  fieldErrors?: Record<string, string[]>;
  status?: number;
}

export function extractApiError(err: unknown): ApiErrorInfo {
  if (axios.isAxiosError(err) && err.response) {
    const { status, data } = err.response;
    if (data && typeof data === "object") {
      // ValidationProblemDetails
      if ("errors" in data && data.errors && typeof data.errors === "object") {
        return {
          status,
          message: (data as { title?: string }).title ?? "Validation failed.",
          fieldErrors: data.errors as Record<string, string[]>,
        };
      }
      // ErrorResponse
      if ("message" in data) {
        return { status, message: String((data as { message: unknown }).message) };
      }
    }
    return { status, message: err.message };
  }
  return { message: err instanceof Error ? err.message : "Unknown error" };
}
```

- [ ] **Step 8: Build (type-check)**

```powershell
cd src/EventManagement.Web
npm run build
cd ../..
```

Expected: TypeScript compiles with no errors. (The dev artifacts in `dist/` will be ignored by `.gitignore`.)

- [ ] **Step 9: Commit**

```powershell
git add src/EventManagement.Web/src
git commit -m "feat(web): add API client, types, and shared utilities"
```

---

## Task 14: Shared components (Spinner, EmptyState, ErrorBanner, ConfirmModal)

**Files:**
- Create: `src/EventManagement.Web/src/shared/components/Spinner.tsx`
- Create: `src/EventManagement.Web/src/shared/components/Spinner.module.css`
- Create: `src/EventManagement.Web/src/shared/components/EmptyState.tsx`
- Create: `src/EventManagement.Web/src/shared/components/EmptyState.module.css`
- Create: `src/EventManagement.Web/src/shared/components/ErrorBanner.tsx`
- Create: `src/EventManagement.Web/src/shared/components/ErrorBanner.module.css`
- Create: `src/EventManagement.Web/src/shared/components/ConfirmModal.tsx`
- Create: `src/EventManagement.Web/src/shared/components/ConfirmModal.module.css`

- [ ] **Step 1: Spinner**

`Spinner.tsx`:
```tsx
import styles from "./Spinner.module.css";

export function Spinner({ label = "Loading..." }: { label?: string }) {
  return (
    <div className={styles.wrapper} role="status" aria-live="polite">
      <div className={styles.dot} />
      <span className="muted">{label}</span>
    </div>
  );
}
```

`Spinner.module.css`:
```css
.wrapper { display: flex; align-items: center; gap: 0.5rem; padding: 1rem 0; }
.dot {
  width: 14px; height: 14px; border-radius: 50%;
  border: 2px solid var(--color-border);
  border-top-color: var(--color-primary);
  animation: spin 0.8s linear infinite;
}
@keyframes spin { to { transform: rotate(360deg); } }
```

- [ ] **Step 2: EmptyState**

`EmptyState.tsx`:
```tsx
import type { ReactNode } from "react";
import styles from "./EmptyState.module.css";

export function EmptyState({ title, action }: { title: string; action?: ReactNode }) {
  return (
    <div className={styles.wrapper}>
      <p className="muted">{title}</p>
      {action}
    </div>
  );
}
```

`EmptyState.module.css`:
```css
.wrapper {
  text-align: center;
  padding: 2rem 1rem;
  border: 1px dashed var(--color-border);
  border-radius: var(--radius);
  display: flex;
  flex-direction: column;
  gap: 1rem;
  align-items: center;
}
```

- [ ] **Step 3: ErrorBanner**

`ErrorBanner.tsx`:
```tsx
import styles from "./ErrorBanner.module.css";

export function ErrorBanner({ message, onRetry }: { message: string; onRetry?: () => void }) {
  return (
    <div className={styles.wrapper} role="alert">
      <span>{message}</span>
      {onRetry && (
        <button type="button" className="button button--ghost" onClick={onRetry}>
          Retry
        </button>
      )}
    </div>
  );
}
```

`ErrorBanner.module.css`:
```css
.wrapper {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 0.5rem;
  background: #fee2e2;
  color: #991b1b;
  border: 1px solid #fecaca;
  border-radius: var(--radius);
  padding: 0.75rem 1rem;
  margin-bottom: 1rem;
}
```

- [ ] **Step 4: ConfirmModal**

`ConfirmModal.tsx`:
```tsx
import styles from "./ConfirmModal.module.css";

interface Props {
  open: boolean;
  title: string;
  message: string;
  confirmLabel?: string;
  onConfirm: () => void;
  onCancel: () => void;
}

export function ConfirmModal({ open, title, message, confirmLabel = "Confirm", onConfirm, onCancel }: Props) {
  if (!open) return null;
  return (
    <div className={styles.backdrop} role="dialog" aria-modal="true">
      <div className={styles.modal}>
        <h2 className={styles.title}>{title}</h2>
        <p className="muted">{message}</p>
        <div className={styles.actions}>
          <button type="button" className="button" onClick={onCancel}>Cancel</button>
          <button type="button" className="button button--danger" onClick={onConfirm}>{confirmLabel}</button>
        </div>
      </div>
    </div>
  );
}
```

`ConfirmModal.module.css`:
```css
.backdrop {
  position: fixed; inset: 0;
  background: rgba(15, 23, 42, 0.5);
  display: flex; align-items: center; justify-content: center;
  padding: 1rem;
  z-index: 50;
}
.modal {
  background: var(--color-surface);
  border-radius: var(--radius);
  padding: 1.5rem;
  width: 100%; max-width: 380px;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.25);
}
.title { margin: 0 0 0.5rem; font-size: 1.1rem; }
.actions { display: flex; justify-content: flex-end; gap: 0.5rem; margin-top: 1.25rem; }
```

- [ ] **Step 5: Commit**

```powershell
git add src/EventManagement.Web/src/shared
git commit -m "feat(web): add shared UI components (Spinner, EmptyState, ErrorBanner, ConfirmModal)"
```

---

## Task 15: Events feature — list page, create page, and shared form

**Files:**
- Create: `src/EventManagement.Web/src/features/events/hooks/useEvents.ts`
- Create: `src/EventManagement.Web/src/features/events/hooks/useEvent.ts`
- Create: `src/EventManagement.Web/src/features/events/hooks/useCreateEvent.ts`
- Create: `src/EventManagement.Web/src/features/events/hooks/useUpdateEvent.ts`
- Create: `src/EventManagement.Web/src/features/events/components/EventCard.tsx`
- Create: `src/EventManagement.Web/src/features/events/components/EventCard.module.css`
- Create: `src/EventManagement.Web/src/features/events/components/EventForm.tsx`
- Create: `src/EventManagement.Web/src/features/events/components/EventForm.module.css`
- Create: `src/EventManagement.Web/src/features/events/pages/EventListPage.tsx`
- Create: `src/EventManagement.Web/src/features/events/pages/CreateEventPage.tsx`
- Modify: `src/EventManagement.Web/src/App.tsx` (wire list + create routes)

- [ ] **Step 1: Create event hooks**

`useEvents.ts`:
```ts
import { useQuery } from "@tanstack/react-query";
import { eventsApi } from "../../../api/eventsApi";

export function useEvents() {
  return useQuery({ queryKey: ["events"], queryFn: eventsApi.list });
}
```

`useEvent.ts`:
```ts
import { useQuery } from "@tanstack/react-query";
import { eventsApi } from "../../../api/eventsApi";

export function useEvent(id: string | undefined) {
  return useQuery({
    queryKey: ["events", id],
    queryFn: () => eventsApi.get(id!),
    enabled: !!id,
  });
}
```

`useCreateEvent.ts`:
```ts
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { eventsApi, type CreateEventPayload } from "../../../api/eventsApi";

export function useCreateEvent() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateEventPayload) => eventsApi.create(payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["events"] }),
  });
}
```

`useUpdateEvent.ts`:
```ts
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { eventsApi, type CreateEventPayload } from "../../../api/eventsApi";

export function useUpdateEvent(id: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateEventPayload) => eventsApi.update(id, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["events"] });
      qc.invalidateQueries({ queryKey: ["events", id] });
    },
  });
}
```

- [ ] **Step 2: EventCard component**

`EventCard.tsx`:
```tsx
import { Link } from "react-router-dom";
import type { EventSummary } from "../types/event";
import { formatDate } from "../../../shared/utils/date";
import styles from "./EventCard.module.css";

export function EventCard({ event }: { event: EventSummary }) {
  const full = event.currentRegistrations >= event.maxCapacity;
  const past = new Date(event.date).getTime() < Date.now();
  return (
    <Link to={`/events/${event.id}`} className={styles.card}>
      <div className={styles.title}>{event.title}</div>
      <div className="muted">{formatDate(event.date)}</div>
      <div className={styles.meta}>
        <span>{event.currentRegistrations} / {event.maxCapacity} registered</span>
        {full && !past && <span className={styles.badgeFull}>Full</span>}
        {past && <span className={styles.badgePast}>Past</span>}
      </div>
    </Link>
  );
}
```

`EventCard.module.css`:
```css
.card {
  display: block;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius);
  padding: 1rem;
  margin-bottom: 0.75rem;
  color: var(--color-text);
  text-decoration: none;
  box-shadow: var(--shadow-sm);
}
.card:hover { border-color: var(--color-primary); text-decoration: none; }
.title { font-weight: 600; font-size: 1.05rem; margin-bottom: 0.25rem; }
.meta { display: flex; gap: 0.5rem; align-items: center; margin-top: 0.5rem; font-size: 0.9rem; color: var(--color-muted); }
.badgeFull, .badgePast { padding: 0.1rem 0.5rem; border-radius: 999px; font-size: 0.75rem; font-weight: 600; }
.badgeFull { background: #fef3c7; color: #92400e; }
.badgePast { background: #e5e7eb; color: #374151; }
```

- [ ] **Step 3: EventForm (shared by Create and Edit)**

`EventForm.tsx`:
```tsx
import { useState, type FormEvent } from "react";
import type { EventDetail, EventFormValues } from "../types/event";
import { toLocalInputValue, fromLocalInputValue } from "../../../shared/utils/date";
import { extractApiError } from "../../../shared/utils/apiError";
import styles from "./EventForm.module.css";

interface Props {
  mode: "create" | "edit";
  initialData?: EventDetail;
  onSubmit: (payload: { title: string; description: string | null; date: string; maxCapacity: number }) => Promise<unknown>;
  onCancel?: () => void;
  submitLabel?: string;
}

export function EventForm({ mode, initialData, onSubmit, onCancel, submitLabel }: Props) {
  const [values, setValues] = useState<EventFormValues>({
    title: initialData?.title ?? "",
    description: initialData?.description ?? "",
    date: initialData ? toLocalInputValue(initialData.date) : "",
    maxCapacity: initialData?.maxCapacity ?? 10,
  });
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<Record<string, string[]>>({});

  function set<K extends keyof EventFormValues>(key: K, value: EventFormValues[K]) {
    setValues((v) => ({ ...v, [key]: value }));
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setSubmitting(true);
    setError(null);
    setFieldErrors({});
    try {
      await onSubmit({
        title: values.title.trim(),
        description: values.description.trim() === "" ? null : values.description.trim(),
        date: fromLocalInputValue(values.date),
        maxCapacity: Number(values.maxCapacity),
      });
    } catch (err) {
      const info = extractApiError(err);
      setError(info.message);
      if (info.fieldErrors) setFieldErrors(info.fieldErrors);
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <form className={styles.form} onSubmit={handleSubmit} noValidate>
      <div className="form-row">
        <label className="label" htmlFor="title">Title</label>
        <input id="title" className="input" required maxLength={200}
          value={values.title} onChange={(e) => set("title", e.target.value)} />
        {fieldErrors.Title?.map((m) => <span key={m} className="field-error">{m}</span>)}
      </div>
      <div className="form-row">
        <label className="label" htmlFor="description">Description</label>
        <textarea id="description" className="textarea" maxLength={2000}
          value={values.description} onChange={(e) => set("description", e.target.value)} />
        {fieldErrors.Description?.map((m) => <span key={m} className="field-error">{m}</span>)}
      </div>
      <div className="form-row">
        <label className="label" htmlFor="date">Date and time</label>
        <input id="date" type="datetime-local" className="input" required
          value={values.date} onChange={(e) => set("date", e.target.value)} />
        {fieldErrors.Date?.map((m) => <span key={m} className="field-error">{m}</span>)}
      </div>
      <div className="form-row">
        <label className="label" htmlFor="maxCapacity">Max capacity</label>
        <input id="maxCapacity" type="number" min={1} className="input" required
          value={values.maxCapacity} onChange={(e) => set("maxCapacity", Number(e.target.value))} />
        {fieldErrors.MaxCapacity?.map((m) => <span key={m} className="field-error">{m}</span>)}
      </div>

      {error && <div className="field-error" role="alert">{error}</div>}

      <div className={styles.actions}>
        {onCancel && (
          <button type="button" className="button" onClick={onCancel} disabled={submitting}>
            Cancel
          </button>
        )}
        <button type="submit" className="button button--primary" disabled={submitting}>
          {submitting ? "Saving..." : submitLabel ?? (mode === "create" ? "Create event" : "Save changes")}
        </button>
      </div>
    </form>
  );
}
```

`EventForm.module.css`:
```css
.form { background: var(--color-surface); border: 1px solid var(--color-border); border-radius: var(--radius); padding: 1.25rem; }
.actions { display: flex; justify-content: flex-end; gap: 0.5rem; margin-top: 0.5rem; }
```

- [ ] **Step 4: EventListPage**

`EventListPage.tsx`:
```tsx
import { Link } from "react-router-dom";
import { useEvents } from "../hooks/useEvents";
import { EventCard } from "../components/EventCard";
import { Spinner } from "../../../shared/components/Spinner";
import { EmptyState } from "../../../shared/components/EmptyState";
import { ErrorBanner } from "../../../shared/components/ErrorBanner";

export function EventListPage() {
  const { data, isLoading, isError, refetch } = useEvents();

  return (
    <section>
      <div className="page-header">
        <h1>Events</h1>
        <Link to="/events/new" className="button button--primary">New event</Link>
      </div>

      {isLoading && <Spinner label="Loading events..." />}
      {isError && <ErrorBanner message="Failed to load events." onRetry={() => refetch()} />}
      {data && data.length === 0 && (
        <EmptyState
          title="No events yet."
          action={<Link to="/events/new" className="button button--primary">Create your first event</Link>}
        />
      )}
      {data && data.length > 0 && (
        <div>{data.map((ev) => <EventCard key={ev.id} event={ev} />)}</div>
      )}
    </section>
  );
}
```

- [ ] **Step 5: CreateEventPage**

`CreateEventPage.tsx`:
```tsx
import { useNavigate, Link } from "react-router-dom";
import toast from "react-hot-toast";
import { EventForm } from "../components/EventForm";
import { useCreateEvent } from "../hooks/useCreateEvent";

export function CreateEventPage() {
  const navigate = useNavigate();
  const mutation = useCreateEvent();

  return (
    <section>
      <div className="page-header">
        <h1>New event</h1>
        <Link to="/" className="button button--ghost">Back</Link>
      </div>
      <EventForm
        mode="create"
        onSubmit={async (payload) => {
          const created = await mutation.mutateAsync(payload);
          toast.success("Event created");
          navigate(`/events/${created.id}`);
        }}
        onCancel={() => navigate("/")}
      />
    </section>
  );
}
```

- [ ] **Step 6: Wire routes in `App.tsx`**

Replace `src/EventManagement.Web/src/App.tsx`:

```tsx
import { Routes, Route, Link } from "react-router-dom";
import { EventListPage } from "./features/events/pages/EventListPage";
import { CreateEventPage } from "./features/events/pages/CreateEventPage";

function Placeholder({ title }: { title: string }) {
  return <div className="card">{title} — coming soon</div>;
}

export default function App() {
  return (
    <div className="app-shell">
      <header className="page-header">
        <Link to="/" style={{ fontWeight: 700, fontSize: "1.25rem", color: "var(--color-text)" }}>
          Event Manager
        </Link>
      </header>
      <Routes>
        <Route path="/" element={<EventListPage />} />
        <Route path="/events/new" element={<CreateEventPage />} />
        <Route path="/events/:id" element={<Placeholder title="Event Detail" />} />
      </Routes>
    </div>
  );
}
```

- [ ] **Step 7: Type-check**

```powershell
cd src/EventManagement.Web
npm run build
cd ../..
```

Expected: TypeScript compiles with no errors.

- [ ] **Step 8: Smoke test**

In one terminal: `dotnet run --project src/EventManagement.Api`
In another: `cd src/EventManagement.Web && npm run dev`

Open `http://localhost:5173`. Expected: empty state. Click "Create your first event", fill in a future-dated event, submit. Expected: redirect to `/events/:id` placeholder; toast "Event created"; back on `/` shows the new event card.

- [ ] **Step 9: Commit**

```powershell
git add src/EventManagement.Web/src
git commit -m "feat(web): add events list, create flow, and shared EventForm"
```

---

## Task 16: Event detail page + registrations feature

**Files:**
- Create: `src/EventManagement.Web/src/features/registrations/hooks/useRegistrations.ts`
- Create: `src/EventManagement.Web/src/features/registrations/hooks/useRegister.ts`
- Create: `src/EventManagement.Web/src/features/registrations/hooks/useUnregister.ts`
- Create: `src/EventManagement.Web/src/features/registrations/components/RegistrationList.tsx`
- Create: `src/EventManagement.Web/src/features/registrations/components/RegistrationList.module.css`
- Create: `src/EventManagement.Web/src/features/registrations/components/RegisterForm.tsx`
- Create: `src/EventManagement.Web/src/features/registrations/components/RegisterForm.module.css`
- Create: `src/EventManagement.Web/src/features/events/pages/EventDetailPage.tsx`
- Create: `src/EventManagement.Web/src/features/events/pages/EventDetailPage.module.css`
- Modify: `src/EventManagement.Web/src/App.tsx` (wire detail route)

- [ ] **Step 1: Registration hooks**

`useRegistrations.ts`:
```ts
import { useQuery } from "@tanstack/react-query";
import { registrationsApi } from "../../../api/registrationsApi";

export function useRegistrations(eventId: string | undefined) {
  return useQuery({
    queryKey: ["events", eventId, "registrations"],
    queryFn: () => registrationsApi.list(eventId!),
    enabled: !!eventId,
  });
}
```

`useRegister.ts`:
```ts
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { registrationsApi } from "../../../api/registrationsApi";
import type { RegisterUserPayload } from "../types/registration";

export function useRegister(eventId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (payload: RegisterUserPayload) => registrationsApi.register(eventId, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["events", eventId] });
      qc.invalidateQueries({ queryKey: ["events", eventId, "registrations"] });
      qc.invalidateQueries({ queryKey: ["events"] });
    },
  });
}
```

`useUnregister.ts`:
```ts
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { registrationsApi } from "../../../api/registrationsApi";

export function useUnregister(eventId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (registrationId: string) => registrationsApi.unregister(eventId, registrationId),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["events", eventId] });
      qc.invalidateQueries({ queryKey: ["events", eventId, "registrations"] });
      qc.invalidateQueries({ queryKey: ["events"] });
    },
  });
}
```

- [ ] **Step 2: RegisterForm**

`RegisterForm.tsx`:
```tsx
import { useState, type FormEvent } from "react";
import toast from "react-hot-toast";
import { useRegister } from "../hooks/useRegister";
import { extractApiError } from "../../../shared/utils/apiError";
import styles from "./RegisterForm.module.css";

export function RegisterForm({ eventId, disabled }: { eventId: string; disabled?: boolean }) {
  const [userId, setUserId] = useState("");
  const [userName, setUserName] = useState("");
  const [error, setError] = useState<string | null>(null);
  const mutation = useRegister(eventId);

  async function onSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    try {
      await mutation.mutateAsync({ userId: userId.trim(), userName: userName.trim() });
      toast.success("Registered");
      setUserId("");
      setUserName("");
    } catch (err) {
      setError(extractApiError(err).message);
    }
  }

  return (
    <form className={styles.form} onSubmit={onSubmit}>
      <h3 className={styles.heading}>Register a new attendee</h3>
      <div className="form-row">
        <label className="label" htmlFor="userId">User ID</label>
        <input id="userId" className="input" required maxLength={100}
          value={userId} onChange={(e) => setUserId(e.target.value)} disabled={disabled} />
      </div>
      <div className="form-row">
        <label className="label" htmlFor="userName">User name</label>
        <input id="userName" className="input" required maxLength={200}
          value={userName} onChange={(e) => setUserName(e.target.value)} disabled={disabled} />
      </div>
      {error && <div className="field-error" role="alert">{error}</div>}
      <button type="submit" className="button button--primary" disabled={disabled || mutation.isPending}>
        {mutation.isPending ? "Registering..." : "Register"}
      </button>
    </form>
  );
}
```

`RegisterForm.module.css`:
```css
.form { background: var(--color-surface); border: 1px solid var(--color-border); border-radius: var(--radius); padding: 1.25rem; }
.heading { margin: 0 0 0.75rem; font-size: 1rem; }
```

- [ ] **Step 3: RegistrationList**

`RegistrationList.tsx`:
```tsx
import { useState } from "react";
import toast from "react-hot-toast";
import { useRegistrations } from "../hooks/useRegistrations";
import { useUnregister } from "../hooks/useUnregister";
import { Spinner } from "../../../shared/components/Spinner";
import { EmptyState } from "../../../shared/components/EmptyState";
import { ErrorBanner } from "../../../shared/components/ErrorBanner";
import { ConfirmModal } from "../../../shared/components/ConfirmModal";
import { formatDate } from "../../../shared/utils/date";
import { extractApiError } from "../../../shared/utils/apiError";
import styles from "./RegistrationList.module.css";

export function RegistrationList({ eventId }: { eventId: string }) {
  const { data, isLoading, isError, refetch } = useRegistrations(eventId);
  const unregister = useUnregister(eventId);
  const [pendingId, setPendingId] = useState<string | null>(null);

  if (isLoading) return <Spinner label="Loading registrations..." />;
  if (isError) return <ErrorBanner message="Failed to load registrations." onRetry={() => refetch()} />;
  if (!data || data.length === 0) return <EmptyState title="No one has registered yet." />;

  return (
    <>
      <ul className={styles.list}>
        {data.map((r) => (
          <li key={r.id} className={styles.row}>
            <div>
              <div className={styles.name}>{r.userName}</div>
              <div className="muted">{r.userId} · {formatDate(r.registeredAt)}</div>
            </div>
            <button type="button" className="button button--ghost" onClick={() => setPendingId(r.id)}>
              Unregister
            </button>
          </li>
        ))}
      </ul>
      <ConfirmModal
        open={pendingId !== null}
        title="Unregister attendee?"
        message="The attendee will be removed from this event."
        confirmLabel="Unregister"
        onCancel={() => setPendingId(null)}
        onConfirm={async () => {
          const id = pendingId!;
          setPendingId(null);
          try {
            await unregister.mutateAsync(id);
            toast.success("Unregistered");
          } catch (err) {
            toast.error(extractApiError(err).message);
          }
        }}
      />
    </>
  );
}
```

`RegistrationList.module.css`:
```css
.list { list-style: none; padding: 0; margin: 0 0 1rem; }
.row {
  display: flex; justify-content: space-between; align-items: center; gap: 1rem;
  padding: 0.75rem 1rem;
  border: 1px solid var(--color-border);
  border-radius: var(--radius);
  background: var(--color-surface);
  margin-bottom: 0.5rem;
}
.name { font-weight: 600; }
```

> Note on the literal `·`: replace with the middle-dot character `·` if your editor supports UTF-8 cleanly; otherwise leave the escape, which renders correctly.

- [ ] **Step 4: EventDetailPage with inline edit**

`EventDetailPage.tsx`:
```tsx
import { useState } from "react";
import { useParams, Link, useNavigate } from "react-router-dom";
import toast from "react-hot-toast";
import { useEvent } from "../hooks/useEvent";
import { useUpdateEvent } from "../hooks/useUpdateEvent";
import { EventForm } from "../components/EventForm";
import { RegisterForm } from "../../registrations/components/RegisterForm";
import { RegistrationList } from "../../registrations/components/RegistrationList";
import { Spinner } from "../../../shared/components/Spinner";
import { ErrorBanner } from "../../../shared/components/ErrorBanner";
import { formatDate } from "../../../shared/utils/date";
import styles from "./EventDetailPage.module.css";

export function EventDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: ev, isLoading, isError, refetch } = useEvent(id);
  const updateMutation = useUpdateEvent(id!);
  const [editing, setEditing] = useState(false);

  if (isLoading) return <Spinner label="Loading event..." />;
  if (isError || !ev) return <ErrorBanner message="Failed to load event." onRetry={() => refetch()} />;

  const past = new Date(ev.date).getTime() < Date.now();
  const full = ev.currentRegistrations >= ev.maxCapacity;
  const canRegister = !past && !full;

  return (
    <section>
      <div className="page-header">
        <Link to="/" className="button button--ghost">← Back</Link>
        {!editing && <button type="button" className="button" onClick={() => setEditing(true)}>Edit</button>}
      </div>

      {!editing ? (
        <div className={`card ${styles.summary}`}>
          <h1 className={styles.title}>{ev.title}</h1>
          <div className="muted">{formatDate(ev.date)}</div>
          {ev.description && <p>{ev.description}</p>}
          <div className={styles.meta}>
            <span>{ev.currentRegistrations} / {ev.maxCapacity} registered</span>
            {past && <span className={styles.badgePast}>Past</span>}
            {full && !past && <span className={styles.badgeFull}>Full</span>}
          </div>
        </div>
      ) : (
        <EventForm
          mode="edit"
          initialData={ev}
          onSubmit={async (payload) => {
            await updateMutation.mutateAsync(payload);
            toast.success("Event updated");
            setEditing(false);
          }}
          onCancel={() => setEditing(false)}
        />
      )}

      <h2 className={styles.sectionHeading}>Registrations</h2>
      <RegistrationList eventId={ev.id} />

      {canRegister && <RegisterForm eventId={ev.id} />}
      {!canRegister && (
        <div className="card muted">
          {past ? "This event is in the past." : "This event is full."}
        </div>
      )}

      <p className={styles.footerLink}>
        <button type="button" className="button button--ghost" onClick={() => navigate("/")}>Back to all events</button>
      </p>
    </section>
  );
}
```

`EventDetailPage.module.css`:
```css
.summary { margin-bottom: 1.5rem; }
.title { margin: 0 0 0.25rem; }
.meta { display: flex; gap: 0.75rem; align-items: center; margin-top: 1rem; }
.badgeFull, .badgePast { padding: 0.15rem 0.55rem; border-radius: 999px; font-size: 0.75rem; font-weight: 600; }
.badgeFull { background: #fef3c7; color: #92400e; }
.badgePast { background: #e5e7eb; color: #374151; }
.sectionHeading { margin: 1.5rem 0 0.75rem; font-size: 1.15rem; }
.footerLink { margin-top: 1.5rem; }
```

> The two `←` characters above render as `←`. If your editor handles UTF-8, you may replace them.

- [ ] **Step 5: Wire detail route**

Update `src/EventManagement.Web/src/App.tsx`:

```tsx
import { Routes, Route, Link } from "react-router-dom";
import { EventListPage } from "./features/events/pages/EventListPage";
import { CreateEventPage } from "./features/events/pages/CreateEventPage";
import { EventDetailPage } from "./features/events/pages/EventDetailPage";

export default function App() {
  return (
    <div className="app-shell">
      <header className="page-header">
        <Link to="/" style={{ fontWeight: 700, fontSize: "1.25rem", color: "var(--color-text)" }}>
          Event Manager
        </Link>
      </header>
      <Routes>
        <Route path="/" element={<EventListPage />} />
        <Route path="/events/new" element={<CreateEventPage />} />
        <Route path="/events/:id" element={<EventDetailPage />} />
      </Routes>
    </div>
  );
}
```

- [ ] **Step 6: Type-check + smoke test**

```powershell
cd src/EventManagement.Web
npm run build
cd ../..
```

Then run backend + frontend (two terminals) and exercise the full flow:
1. Create an event
2. Open it
3. Register a user
4. Try to register the same user again → expect inline error
5. Unregister → confirm modal → toast
6. Edit the event title → save → toast and refresh

- [ ] **Step 7: Commit**

```powershell
git add src/EventManagement.Web/src
git commit -m "feat(web): add event detail page with inline edit and registration flows"
```

---

## Task 17: README and final verification

**Files:**
- Replace: `README.md`

- [ ] **Step 1: Replace `README.md`**

```markdown
# Event Management

REST API and React SPA for managing events and attendee registrations. Built for the take-home exercise described in `docs/requirements/event-management-requirements-v1.md`.

## Tech stack

- **Backend:** ASP.NET Core 8 (C#), in-memory storage, Swashbuckle (Swagger), `Microsoft.Extensions.Logging`, DataAnnotations, xUnit + FluentAssertions + NSubstitute.
- **Frontend:** React 18 + Vite + TypeScript, react-router-dom v6, TanStack Query, axios, react-hot-toast, plain CSS Modules.

## Solution layout

```
src/
  EventManagement.Api/              ASP.NET Core controllers, middleware, request models
  EventManagement.Application/      Services, DTOs, repository interfaces, IClock
  EventManagement.Domain/           Entities, exceptions, business rules
  EventManagement.Infrastructure/   In-memory repositories, SystemClock, DI extension
  EventManagement.Web/              React + Vite SPA
tests/
  EventManagement.Domain.Tests/     RegistrationRules unit tests
  EventManagement.Application.Tests/ EventService and RegistrationService unit tests
  EventManagement.Api.Tests/        Endpoint integration tests via WebApplicationFactory
```

## Running the backend

```bash
dotnet run --project src/EventManagement.Api
```

The API listens on `http://localhost:5050`. Swagger UI: `http://localhost:5050/swagger`.

## Running the frontend

```bash
cd src/EventManagement.Web
npm install
npm run dev
```

Vite dev server: `http://localhost:5173`. The API base URL is read from `VITE_API_BASE_URL` (defaults to `http://localhost:5050` via `.env.development`).

## Running the tests

```bash
dotnet test
```

19 tests across three test projects.

## API endpoints

| Method | Route | Purpose |
|---|---|---|
| GET | `/api/events` | List events |
| GET | `/api/events/{id}` | Get event by id |
| POST | `/api/events` | Create event |
| PUT | `/api/events/{id}` | Update event |
| GET | `/api/events/{eventId}/registrations` | List registrations for an event |
| POST | `/api/events/{eventId}/registrations` | Register a user |
| DELETE | `/api/events/{eventId}/registrations/{registrationId}` | Unregister a user |

Response codes: 200 (read/update), 201 (create/register), 204 (delete), 400 (DataAnnotations validation), 404 (not found), 422 (business rule violation), 500 (unexpected). 400 responses use ASP.NET Core's `ValidationProblemDetails`; other errors use a small `{ message, statusCode, details }` shape.

CORS: development policy whitelists `http://localhost:5173` only (the Vite dev server).

## Assumptions

1. **DELETE event endpoint is intentionally omitted.** v1 §3 lists only Create/Read/Update for events. v1 §5 says "Standard CRUD" but is contradicted by §3; the strict reading of §3 wins.
2. **`userId` and `userName` come from the request body** because authentication is out of scope per v1 §7.
3. **State resets on process restart.** In-memory storage per v1 §4.
4. **All dates are stored as UTC and displayed in the user's local timezone.**
5. **Logging uses built-in `Microsoft.Extensions.Logging`.** Serilog would be the natural next step for production.
6. **Validation uses DataAnnotations.** FluentValidation would be the natural next step if rules grow complex.
7. **A per-event `SemaphoreSlim` guards the registration critical section.** Sufficient for a single-process in-memory model; would migrate to optimistic concurrency or a DB-level constraint with real persistence.

## Future improvements

- Physical database (EF Core + Postgres or SQLite)
- Pagination on event and registration lists
- Authentication / authorization
- Frontend tests (Vitest + RTL)
- Docker compose for backend + frontend
- Serilog with structured sinks
- FluentValidation once rules grow

## Design and plan documents

- Spec: `docs/superpowers/specs/2026-05-14-event-management-design.md`
- Implementation plan: `docs/superpowers/plans/2026-05-14-event-management.md`
```

- [ ] **Step 2: Final test run**

```powershell
dotnet test
```

Expected: 19 tests pass.

- [ ] **Step 3: Manual verification checklist**

Run backend (`dotnet run --project src/EventManagement.Api`) and frontend (`cd src/EventManagement.Web && npm run dev`) in two terminals. Verify in the UI **and** via Swagger:

- [ ] Create an event with a valid future date → 201 in Swagger; appears in the list on `/`
- [ ] Create with empty title → 400 in Swagger; inline error on the form
- [ ] Open event detail page; shows event + empty registrations + register form
- [ ] Register `user-1` → 201; appears in the list; toast "Registered"
- [ ] Register `user-1` again → 422 in Swagger; inline error on the form
- [ ] Set capacity to 1 (via Edit), register a second user → 422
- [ ] Use Swagger to PUT an event into the past, then attempt to register → 422
- [ ] Unregister `user-1` → confirm modal → 204; toast "Unregistered"
- [ ] Edit the event from the detail page → save → toast and refreshed values
- [ ] GET `/api/events/{unknown-guid}` → 404
- [ ] Empty event list shows the empty state with "Create your first event"

- [ ] **Step 4: Commit**

```powershell
git add README.md
git commit -m "docs: rewrite README with run instructions, endpoints, and assumptions"
```

---

## Self-review checklist (for the planner — not the implementer)

Already performed against the spec:

1. **Spec coverage** — every section of the spec has a corresponding task:
   - §1 Goal → all tasks
   - §2 Scope cuts → Tasks 15-16 (Edit merged into Detail), Task 10 (`UseHttpLogging` not custom middleware)
   - §3 Stack → Tasks 1, 12 (deps), Task 10 (Serilog absent, HttpLogging present), Task 8 (DataAnnotations), Task 12 (Tailwind absent, CSS Modules present)
   - §3.4 Ports/CORS → Task 10 (CORS policy) + Task 12 (`VITE_API_BASE_URL`)
   - §4 Solution layout → Task 1
   - §5 Entities → Task 2
   - §6 Business rules → Task 3
   - §7 Concurrency → Task 6 (`ConcurrentDictionary<Guid, SemaphoreSlim>`)
   - §8 Repos → Task 7
   - §9 Services → Tasks 5 + 6
   - §10 Request/response models → Tasks 4 (DTOs) + 8 (request models with DataAnnotations)
   - §11 API contract → Task 9 (controllers) + Task 10 (status codes via middleware)
   - §12 Exception handling → Task 8 (middleware)
   - §13 Logging → Task 10 (`appsettings.json` + `UseHttpLogging`)
   - §14 Frontend pages → Tasks 13-16
   - §15 Testing → Tasks 3, 5, 6, 11
   - §16 README → Task 17
   - §17 Assumptions → Task 17 (README)
   - §18 Manual checklist → Task 17 Step 3
2. **No placeholders** — every step has concrete code or commands.
3. **Type consistency** — `EventDto`, `EventDetailDto`, `RegistrationDto`, `CreateEventInput`, `UpdateEventInput`, `RegisterUserInput`, query keys `["events"]`/`["events", id]`/`["events", id, "registrations"]`, and method names match across tasks.
