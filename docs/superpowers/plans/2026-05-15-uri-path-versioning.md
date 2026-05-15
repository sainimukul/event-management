# URI Path Versioning Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Move every existing `EventManagement.Api` endpoint behind a `/api/v1/...` URL prefix using `Asp.Versioning.Mvc`, with Swagger version dropdown support and a hard cut on unversioned routes.

**Architecture:** Add the `Asp.Versioning.Mvc` + `Asp.Versioning.Mvc.ApiExplorer` packages, configure URL-segment versioning in `Program.cs`, decorate both controllers with `[ApiVersion("1.0")]` and `[Route("api/v{version:apiVersion}/...")]`, register a `ConfigureSwaggerOptions` class that drives Swagger doc registration from `IApiVersionDescriptionProvider`. Frontend moves the `v1` literal into the axios `baseURL` so future versions are a single-line change.

**Tech Stack:** ASP.NET Core 8, `Asp.Versioning.Mvc 8.x`, `Asp.Versioning.Mvc.ApiExplorer 8.x`, Swashbuckle, axios.

**Source spec:** `docs/superpowers/specs/2026-05-15-uri-path-versioning-design.md` (commit `7ab5ed2`).

**Commit boundaries (per spec §13):** Two commits total — Task 1 ends with the backend commit, Task 2 ends with the frontend + docs commit.

**Time estimate:** ~45 minutes total (~30 backend, ~15 frontend + docs).

---

## Pre-flight check

Run from repo root before starting:

```powershell
git status
dotnet test
```

Expected: working tree clean (or only untracked tool caches), 19 tests passing. If either condition fails, stop and resolve before proceeding.

---

## Task 1: Backend versioning

**Files:**
- Modify: `src/EventManagement.Api/EventManagement.Api.csproj` (add 2 package refs)
- Create: `src/EventManagement.Api/ConfigureSwaggerOptions.cs`
- Modify: `src/EventManagement.Api/Program.cs` (versioning registration, Swagger UI loop)
- Modify: `src/EventManagement.Api/Controllers/EventsController.cs` (add `[ApiVersion]`, change `[Route]`)
- Modify: `src/EventManagement.Api/Controllers/RegistrationsController.cs` (add `[ApiVersion]`, change `[Route]`, update hardcoded Location string)
- Create: `tests/EventManagement.Api.Tests/ApiVersioningTests.cs`
- Modify: `tests/EventManagement.Api.Tests/EventsControllerTests.cs` (3 path string updates)
- Modify: `tests/EventManagement.Api.Tests/RegistrationsControllerTests.cs` (7 path string updates)

- [ ] **Step 1: Add Asp.Versioning packages**

```powershell
dotnet add src/EventManagement.Api/EventManagement.Api.csproj package Asp.Versioning.Mvc --version 8.*
dotnet add src/EventManagement.Api/EventManagement.Api.csproj package Asp.Versioning.Mvc.ApiExplorer --version 8.*
```

If `8.*` fails to resolve, retry with the latest stable `8.x` shown by `dotnet search Asp.Versioning.Mvc` (e.g. `--version 8.1.0`).

Verify the references landed:

```powershell
dotnet restore src/EventManagement.Api
```

Expected: no errors, `obj/project.assets.json` regenerated.

- [ ] **Step 2: Write `ApiVersioningTests.cs` (RED — currently fails)**

Create `tests/EventManagement.Api.Tests/ApiVersioningTests.cs`:

```csharp
using System.Net;
using EventManagement.Api.Tests.Helpers;
using FluentAssertions;

namespace EventManagement.Api.Tests;

public class ApiVersioningTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    public ApiVersioningTests(ApiFactory factory) => _factory = factory;

    [Fact]
    public async Task GET_unversioned_events_path_returns_404()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/events");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GET_unsupported_version_returns_400()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v999/events");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
```

- [ ] **Step 3: Run the new tests to verify RED**

```powershell
dotnet test tests/EventManagement.Api.Tests --filter "FullyQualifiedName~ApiVersioningTests"
```

Expected: both tests **FAIL**.
- `GET_unversioned_events_path_returns_404` fails because `/api/events` currently returns 200 (the route still matches `EventsController`).
- `GET_unsupported_version_returns_400` fails because `/api/v999/events` currently returns 404 (no route matches), not 400.

- [ ] **Step 4: Create `ConfigureSwaggerOptions.cs`**

Create `src/EventManagement.Api/ConfigureSwaggerOptions.cs`:

```csharp
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EventManagement.Api;

public sealed class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) => _provider = provider;

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, new OpenApiInfo
            {
                Title = "Event Management API",
                Version = description.ApiVersion.ToString(),
            });
        }
    }
}
```

- [ ] **Step 5: Rewrite `Program.cs`**

Replace the entire contents of `src/EventManagement.Api/Program.cs` with:

```csharp
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using EventManagement.Api;
using EventManagement.Api.Middleware;
using EventManagement.Infrastructure;
using Microsoft.AspNetCore.HttpLogging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddApiVersioning(o =>
{
    o.DefaultApiVersion = new ApiVersion(1, 0);
    o.ReportApiVersions = true;
    o.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddApiExplorer(o =>
{
    o.GroupNameFormat = "'v'VVV";
    o.SubstituteApiVersionInUrl = true;
});

builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

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
    app.UseSwaggerUI(o =>
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        foreach (var description in provider.ApiVersionDescriptions)
        {
            o.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                description.GroupName.ToUpperInvariant());
        }
    });
    app.UseCors(CorsPolicy);
}

app.MapControllers();

app.Run();

public partial class Program { }
```

Key changes vs. the previous `Program.cs`:
- Added `using Asp.Versioning;` and `using Asp.Versioning.ApiExplorer;` at top.
- Added `AddApiVersioning(...)` registration before `AddSwaggerGen()`.
- `AddSwaggerGen()` now has no inline `SwaggerDoc` call — `ConfigureSwaggerOptions` handles it.
- Added `builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();`.
- `UseSwaggerUI` loops over `IApiVersionDescriptionProvider.ApiVersionDescriptions`.

- [ ] **Step 6: Update `EventsController.cs`**

In `src/EventManagement.Api/Controllers/EventsController.cs`, change the attribute block from:

```csharp
[ApiController]
[Route("api/events")]
public sealed class EventsController : ControllerBase
```

To:

```csharp
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/events")]
public sealed class EventsController : ControllerBase
```

Add at the top of the file: `using Asp.Versioning;`

- [ ] **Step 7: Update `RegistrationsController.cs`**

In `src/EventManagement.Api/Controllers/RegistrationsController.cs`, change the attribute block from:

```csharp
[ApiController]
[Route("api/events/{eventId:guid}/registrations")]
public sealed class RegistrationsController : ControllerBase
```

To:

```csharp
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/events/{eventId:guid}/registrations")]
public sealed class RegistrationsController : ControllerBase
```

Add at the top of the file: `using Asp.Versioning;`

Then update line 25's hardcoded Location string from:

```csharp
return Created($"/api/events/{eventId}/registrations/{created.Id}", created);
```

To:

```csharp
return Created($"/api/v1/events/{eventId}/registrations/{created.Id}", created);
```

- [ ] **Step 8: Build to catch wiring errors before running tests**

```powershell
dotnet build
```

Expected: build succeeds with 0 errors.

If it fails with a missing-type error, double-check the `using Asp.Versioning;` and `using Asp.Versioning.ApiExplorer;` directives in `Program.cs` and the two controllers.

- [ ] **Step 9: Run the new `ApiVersioningTests` to verify GREEN**

```powershell
dotnet test tests/EventManagement.Api.Tests --filter "FullyQualifiedName~ApiVersioningTests"
```

Expected: 2 passed (both versioning tests now pass — the unversioned route returns 404, the unsupported version returns 400).

- [ ] **Step 10: Run all tests to confirm existing tests now fail (path mismatch)**

```powershell
dotnet test
```

Expected: 12 unit tests pass (Domain + Application — unchanged), plus 2 new `ApiVersioningTests` pass. The 7 existing API integration tests in `EventsControllerTests` and `RegistrationsControllerTests` should now **FAIL** because their hardcoded `/api/events` paths return 404.

This failure is expected and confirms the route move worked. Proceed to Step 11.

- [ ] **Step 11: Update `EventsControllerTests.cs` path strings**

In `tests/EventManagement.Api.Tests/EventsControllerTests.cs`, replace these three lines:

Line 27 (in `POST_events_with_valid_body_returns_201_with_location_header`):
```csharp
// before
var response = await client.PostAsJsonAsync("/api/events", ValidCreateBody());
// after
var response = await client.PostAsJsonAsync("/api/v1/events", ValidCreateBody());
```

Line 40 (in `GET_events_unknown_id_returns_404`):
```csharp
// before
var response = await client.GetAsync($"/api/events/{Guid.NewGuid()}");
// after
var response = await client.GetAsync($"/api/v1/events/{Guid.NewGuid()}");
```

Line 57 (in `POST_events_with_empty_title_returns_400`):
```csharp
// before
var response = await client.PostAsJsonAsync("/api/events", body);
// after
var response = await client.PostAsJsonAsync("/api/v1/events", body);
```

- [ ] **Step 12: Update `RegistrationsControllerTests.cs` path strings**

In `tests/EventManagement.Api.Tests/RegistrationsControllerTests.cs`, update seven lines. Use Edit's `replace_all` with the literal `"/api/events"` → `"/api/v1/events"` to handle all of them in one pass:

Lines that change:
- Line 24 (in `CreateEvent` helper): `client.PostAsJsonAsync("/api/events", CreateEventBody(date, capacity))` → `client.PostAsJsonAsync("/api/v1/events", CreateEventBody(date, capacity))`
- Line 37 (in `POST_registration_for_future_event_returns_201`): `client.PostAsJsonAsync($"/api/events/{eventId}/registrations", ...)` → `client.PostAsJsonAsync($"/api/v1/events/{eventId}/registrations", ...)`
- Line 51 (in `POST_duplicate_registration_returns_422`): same registration-path update
- Line 53 (same test, second call): same update
- Line 65 (in `POST_registration_when_at_capacity_returns_422`): same update
- Line 67 (same test, second call): same update
- Line 83 (in `POST_registration_for_past_event_returns_422`): same update

Easiest mechanical edit: in your editor, find-and-replace `/api/events` → `/api/v1/events` in this one file. Only this file — do not apply globally.

- [ ] **Step 13: Run all tests — expect 21 passing**

```powershell
dotnet test
```

Expected breakdown:
- `EventManagement.Domain.Tests`: 4 passed
- `EventManagement.Application.Tests`: 8 passed
- `EventManagement.Api.Tests`: 9 passed (3 events + 4 registrations + 2 versioning)
- **Total: 21 passed, 0 failed**

If any test fails, stop and investigate. Do not commit until all 21 pass.

- [ ] **Step 14: Smoke-check the API surface manually (optional but recommended)**

This verifies versioning behavior without opening a browser. In one terminal:

```powershell
dotnet run --project src/EventManagement.Api
```

In another terminal, hit the new routes:

```powershell
# Should return 200 with an empty list
curl http://localhost:5050/api/v1/events
# Should return 404
curl -i http://localhost:5050/api/events
# Should return 400 with ProblemDetails
curl -i http://localhost:5050/api/v999/events
# Swagger UI should show a "V1" version dropdown
# Visit http://localhost:5050/swagger in a browser
```

Stop the server with Ctrl+C.

If you cannot run the server in your environment, skip this step — the test suite covers the behavior.

- [ ] **Step 15: Commit Task 1 (backend)**

```powershell
git add src/EventManagement.Api tests/EventManagement.Api.Tests
git commit -m "$(cat <<'EOF'
feat(api): add URI path versioning with Asp.Versioning

All endpoints move from /api/... to /api/v1/.... Unversioned routes
return 404; unsupported versions return 400 via Asp.Versioning's
default behavior. Swagger UI surfaces a version dropdown sourced from
IApiVersionDescriptionProvider, so future versions need no Swagger
config changes.

Co-Authored-By: Claude Opus 4.7 (1M context) <noreply@anthropic.com>
EOF
)"
```

Verify:

```powershell
git log --oneline -1
```

Expected: shows the new commit.

---

## Task 2: Frontend + docs

**Files:**
- Modify: `src/EventManagement.Web/src/api/client.ts` (append `/api/v1` to baseURL)
- Modify: `src/EventManagement.Web/src/api/eventsApi.ts` (drop `/api/` from 4 path strings)
- Modify: `src/EventManagement.Web/src/api/registrationsApi.ts` (drop `/api/` from 3 path strings)
- Modify: `README.md` (endpoint table + base URL note)
- Modify: `CLAUDE.md` (toolchain note + architecture subsection)

- [ ] **Step 1: Update `client.ts`**

Replace the contents of `src/EventManagement.Web/src/api/client.ts` with:

```ts
import axios from "axios";

const root = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5050";

export const apiClient = axios.create({
  baseURL: `${root}/api/v1`,
  headers: { "Content-Type": "application/json" },
});
```

- [ ] **Step 2: Update `eventsApi.ts`**

In `src/EventManagement.Web/src/api/eventsApi.ts`, replace the four path literals:

```ts
// before
const { data } = await apiClient.get<EventSummary[]>("/api/events");
const { data } = await apiClient.get<EventDetail>(`/api/events/${id}`);
const { data } = await apiClient.post<EventSummary>("/api/events", payload);
const { data } = await apiClient.put<EventSummary>(`/api/events/${id}`, payload);

// after
const { data } = await apiClient.get<EventSummary[]>("/events");
const { data } = await apiClient.get<EventDetail>(`/events/${id}`);
const { data } = await apiClient.post<EventSummary>("/events", payload);
const { data } = await apiClient.put<EventSummary>(`/events/${id}`, payload);
```

The four changes are localized to the four `eventsApi` methods. The signatures of `eventsApi.list/get/create/update` and the imported types are unchanged.

- [ ] **Step 3: Update `registrationsApi.ts`**

In `src/EventManagement.Web/src/api/registrationsApi.ts`, replace the three path literals:

```ts
// before
const { data } = await apiClient.get<Registration[]>(`/api/events/${eventId}/registrations`);
const { data } = await apiClient.post<Registration>(`/api/events/${eventId}/registrations`, payload);
await apiClient.delete(`/api/events/${eventId}/registrations/${registrationId}`);

// after
const { data } = await apiClient.get<Registration[]>(`/events/${eventId}/registrations`);
const { data } = await apiClient.post<Registration>(`/events/${eventId}/registrations`, payload);
await apiClient.delete(`/events/${eventId}/registrations/${registrationId}`);
```

- [ ] **Step 4: Build the frontend to verify type-check**

```powershell
cd src/EventManagement.Web
npm run build
cd ../..
```

Expected: TypeScript compiles cleanly, Vite builds to `dist/`.

- [ ] **Step 5: Update `README.md` endpoint table**

In `README.md`, find the API endpoints table and prepend `/api/v1` to all 7 route entries. The table currently reads:

```markdown
| Method | Route | Purpose |
|---|---|---|
| GET | `/api/events` | List events |
| GET | `/api/events/{id}` | Get event by id |
| POST | `/api/events` | Create event |
| PUT | `/api/events/{id}` | Update event |
| GET | `/api/events/{eventId}/registrations` | List registrations for an event |
| POST | `/api/events/{eventId}/registrations` | Register a user |
| DELETE | `/api/events/{eventId}/registrations/{registrationId}` | Unregister a user |
```

Replace with:

```markdown
| Method | Route | Purpose |
|---|---|---|
| GET | `/api/v1/events` | List events |
| GET | `/api/v1/events/{id}` | Get event by id |
| POST | `/api/v1/events` | Create event |
| PUT | `/api/v1/events/{id}` | Update event |
| GET | `/api/v1/events/{eventId}/registrations` | List registrations for an event |
| POST | `/api/v1/events/{eventId}/registrations` | Register a user |
| DELETE | `/api/v1/events/{eventId}/registrations/{registrationId}` | Unregister a user |
```

Then, immediately above (or below) the table, ensure there is a sentence that reads:

```markdown
The API uses URL-segment versioning (`/api/v{version}/...`). The current version is `v1`. Unversioned routes (e.g. `/api/events`) return `404`; unsupported versions return `400` with a `ProblemDetails` body.
```

If a similar sentence already exists, leave it; otherwise insert it before the table.

- [ ] **Step 6: Update `CLAUDE.md`**

**6a. Add a toolchain bullet.** In `CLAUDE.md`, find the section `## Toolchain notes that bite`. After the existing `Microsoft.AspNetCore.Mvc.Testing` bullet, add a new bullet:

```markdown
- **`Asp.Versioning.Mvc` is pinned to `8.x`** — these packages track the .NET TFM line; `8.x` is the line for `net8.0`. Newer majors require a newer TFM.
```

**6b. Add a versioning subsection.** Find the section `## Architecture: dependency direction is enforced by csproj references`. Below it, immediately above the `## Three patterns that recur` heading, add a new subsection:

```markdown
## API versioning

The API uses URL-segment versioning via `Asp.Versioning.Mvc`. Routes are templated as `[Route("api/v{version:apiVersion}/...")]` and controllers carry `[ApiVersion("1.0")]`. The versioning config lives in `Program.cs` (`AddApiVersioning` + `AddApiExplorer`), and Swagger registers one doc per discovered version via `ConfigureSwaggerOptions`. Adding v2 means decorating new controllers with `[ApiVersion("2.0")]` — no further wiring needed.
```

- [ ] **Step 7: Commit Task 2 (frontend + docs)**

```powershell
git add src/EventManagement.Web/src README.md CLAUDE.md
git commit -m "$(cat <<'EOF'
feat(web): move frontend client to /api/v1 and update docs

Axios baseURL now includes /api/v1 so per-endpoint paths shorten to
/events, /events/{id}/registrations, etc. README endpoint table and
CLAUDE.md tech notes updated to reflect the new versioning scheme.

Co-Authored-By: Claude Opus 4.7 (1M context) <noreply@anthropic.com>
EOF
)"
```

Verify:

```powershell
git log --oneline -2
```

Expected: shows both commits (Task 1 and Task 2) in order.

---

## Post-implementation verification

After Task 2 commits, run a final integrated check from repo root:

- [ ] **Backend:** `dotnet test` — 21 passed.
- [ ] **Frontend:** `cd src/EventManagement.Web && npm run build` — clean build.
- [ ] **Working tree:** `git status` — only tool-cache directories untracked (`.serena/cache`, etc.).
- [ ] **Commit history:** `git log --oneline -3` shows the two new commits plus the previous spec commit.

Optional manual UI smoke test (requires browser):
- Start backend (`dotnet run --project src/EventManagement.Api`) and frontend (`cd src/EventManagement.Web && npm run dev`).
- Visit `http://localhost:5173`, create an event, register a user, edit the event, unregister — all should work as before.
- Open `http://localhost:5050/swagger` — should show "V1" in the version dropdown (top-right).

---

## Self-review checklist (for the planner — not the implementer)

Already performed against the spec:

**1. Spec coverage** — every section maps to a task:

| Spec section | Task / step |
|---|---|
| §1 Goal — versioned routes, Swagger dropdown, hard cut | Task 1 Steps 4-7 (controllers + Swagger), Step 2 (versioning tests) |
| §2.1 Scope: packages | Task 1 Step 1 |
| §2.1 Scope: AddApiVersioning + UrlSegmentApiVersionReader | Task 1 Step 5 |
| §2.1 Scope: controllers `[ApiVersion]` + route templates | Task 1 Steps 6-7 |
| §2.1 Scope: hardcoded Location string | Task 1 Step 7 |
| §2.1 Scope: ConfigureSwaggerOptions | Task 1 Step 4 |
| §2.1 Scope: existing tests path updates | Task 1 Steps 11-12 |
| §2.1 Scope: new ApiVersioningTests | Task 1 Step 2 |
| §2.1 Scope: frontend baseURL + path updates | Task 2 Steps 1-3 |
| §2.1 Scope: README + CLAUDE.md | Task 2 Steps 5-6 |
| §2.2 Out of scope (v2, aliases, header versioning) | Not implemented (correct) |
| §10 Behavior matrix | Task 1 Step 2 (new tests verify 404/400) + Step 14 (optional manual curl smoke) |
| §13 Two commits | Task 1 Step 15 + Task 2 Step 7 |

**2. No placeholders** — every step has concrete code or commands. No "TBD", no "implement later".

**3. Type consistency** — package names, attribute names, route templates, and test paths are identical across the steps that reference them. `[ApiVersion("1.0")]`, `UrlSegmentApiVersionReader`, `IApiVersionDescriptionProvider`, `GroupNameFormat = "'v'VVV"`, `SubstituteApiVersionInUrl = true` — all spelled consistently.
