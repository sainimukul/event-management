# URI Path Versioning — Design Spec

- **Date:** 2026-05-15
- **Status:** Approved — ready for implementation planning
- **Author:** Mukul Saini (with brainstorming via Claude)
- **Builds on:** `docs/superpowers/specs/2026-05-14-event-management-design.md`

This spec captures the agreed design for introducing URI path versioning to `EventManagement.Api`. It is the single source of truth for the implementation; the v1 design spec remains valid for everything not amended here.

---

## 1. Goal

Move every existing endpoint behind a `/api/v1/...` URL prefix using the `Asp.Versioning.Mvc` library, so future versions (`v2`, ...) can coexist on the same host without touching v1 routes or controllers.

This is a forward-looking polish on a take-home submission. There is no real v2 today. The deliverable is:

- Versioned routes for all 7 existing endpoints.
- Swagger UI that shows a version dropdown driven by `IApiVersionDescriptionProvider` (so adding v2 later requires no Swagger config edits).
- Hard cut: unversioned `/api/events` paths return 404.
- Frontend, tests, and docs updated in lockstep.

---

## 2. Scope

### 2.1 In scope

- Add `Asp.Versioning.Mvc` and `Asp.Versioning.Mvc.ApiExplorer` packages to `EventManagement.Api`.
- Configure `AddApiVersioning(...)` with `UrlSegmentApiVersionReader` and `DefaultApiVersion = 1.0`.
- Decorate `EventsController` and `RegistrationsController` with `[ApiVersion("1.0")]` and parameterized route templates (`api/v{version:apiVersion}/...`).
- Replace the one hardcoded Location string in `RegistrationsController` to use the `/api/v1/...` prefix.
- Add a `ConfigureSwaggerOptions` class that registers one Swagger doc per discovered API version; wire `app.UseSwaggerUI(...)` to surface all discovered versions.
- Update the 9 path literals in the existing API integration tests to use `/api/v1/...`.
- Add a small `ApiVersioningTests` class with 2 cases: unversioned URL → 404, unsupported version → 400.
- Update the frontend axios client `baseURL` to append `/api/v1`; shorten paths in `eventsApi.ts` and `registrationsApi.ts` accordingly.
- Update `README.md` and `CLAUDE.md` to reflect the new endpoint paths.

### 2.2 Out of scope

- A second API version (v2, etc.) — no behavior changes, just versioning machinery.
- Backwards-compatibility aliases for `/api/events` (hard cut by decision).
- Other versioning strategies (query string, custom header, accept header). URL-segment only.
- Deprecation tags, sunset headers, or per-action versioning.
- Changes to controller logic, services, repositories, domain rules, or DTOs.
- Frontend behavior changes (no new pages, no new mutations).

### 2.3 Decisions taken during brainstorming

- **Library over manual prefix:** `Asp.Versioning.Mvc` is the standard ASP.NET Core versioning library. A manual prefix would work for a single version but doesn't compose cleanly with Swagger or future versions.
- **Hard cut on unversioned routes:** The only existing client is the in-repo frontend; preserving `/api/events` as an alias would double the surface and confuse Swagger.
- **Version literal lives in axios `baseURL`, not per-endpoint paths:** Future v2 migration becomes a one-line change in `api/client.ts` instead of 7 path edits.

---

## 3. Package additions

In `src/EventManagement.Api/EventManagement.Api.csproj`:

```xml
<ItemGroup>
  <PackageReference Include="Asp.Versioning.Mvc" Version="8.*" />
  <PackageReference Include="Asp.Versioning.Mvc.ApiExplorer" Version="8.*" />
</ItemGroup>
```

Use whichever 8.x release is available on NuGet at implementation time — these packages track .NET TFM majors, so 8.x is the line that targets `net8.0`. If `8.*` doesn't resolve, fall back to the latest stable matching `net8.0` (likely `8.1.0` or newer).

---

## 4. `Program.cs` changes

Add `using Asp.Versioning;` at the top. Insert the versioning registration after `AddSwaggerGen` but before `AddEventManagement` (or anywhere among the service registrations — order doesn't matter):

```csharp
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
```

`UseSwaggerUI(...)` is replaced with a version-aware variant that iterates `IApiVersionDescriptionProvider.ApiVersionDescriptions`:

```csharp
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
```

The previous fixed `c.SwaggerDoc("v1", ...)` call inside `AddSwaggerGen` is removed because `ConfigureSwaggerOptions` (§6) registers docs dynamically.

---

## 5. Controller changes

### `EventsController.cs`

```csharp
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/events")]
public sealed class EventsController : ControllerBase { /* unchanged body */ }
```

### `RegistrationsController.cs`

```csharp
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/events/{eventId:guid}/registrations")]
public sealed class RegistrationsController : ControllerBase { /* unchanged body */ }
```

The one hardcoded Location string at line 25 changes:

```csharp
// before
return Created($"/api/events/{eventId}/registrations/{created.Id}", created);
// after
return Created($"/api/v1/events/{eventId}/registrations/{created.Id}", created);
```

Building the path from route metadata (`Url.Link(...)`) is rejected as over-engineering for one line.

---

## 6. `ConfigureSwaggerOptions`

New file: `src/EventManagement.Api/ConfigureSwaggerOptions.cs`.

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

Registered in `Program.cs`:

```csharp
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();
```

`AddSwaggerGen()` is called with no inline `SwaggerDoc` (the configure-options pattern handles it).

---

## 7. Test updates

### 7.1 Path string updates

| File | Lines | Change |
|---|---|---|
| `tests/EventManagement.Api.Tests/EventsControllerTests.cs` | 27, 40, 57 | `/api/events` → `/api/v1/events` |
| `tests/EventManagement.Api.Tests/RegistrationsControllerTests.cs` | 24, 37, 51, 53, 65, 67, 83 | `/api/events` and `/api/events/{eventId}/registrations` → versioned forms |

No assertion changes — the 19 existing tests continue to verify the same behaviors.

### 7.2 New `ApiVersioningTests.cs`

New file: `tests/EventManagement.Api.Tests/ApiVersioningTests.cs`.

Two cases:

1. **`GET /api/events` (no version) returns 404.** Confirms the hard cut — no route matches an unversioned path.
2. **`GET /api/v999/events` (unsupported version) returns 400.** Confirms `Asp.Versioning`'s default behavior for unknown versions.

Both use `ApiFactory` like the existing tests. Expected final tally: **21 tests pass** (19 existing + 2 new).

---

## 8. Frontend changes

### 8.1 `src/EventManagement.Web/src/api/client.ts`

```ts
const root = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5050";

export const apiClient = axios.create({
  baseURL: `${root}/api/v1`,
  headers: { "Content-Type": "application/json" },
});
```

### 8.2 `eventsApi.ts` and `registrationsApi.ts`

Drop `/api/` from each path string. New shapes:

```ts
// eventsApi.ts
apiClient.get<EventSummary[]>("/events")
apiClient.get<EventDetail>(`/events/${id}`)
apiClient.post<EventSummary>("/events", payload)
apiClient.put<EventSummary>(`/events/${id}`, payload)

// registrationsApi.ts
apiClient.get<Registration[]>(`/events/${eventId}/registrations`)
apiClient.post<Registration>(`/events/${eventId}/registrations`, payload)
apiClient.delete(`/events/${eventId}/registrations/${registrationId}`)
```

### 8.3 `.env.development`

Unchanged. The version literal lives in `client.ts`, not the env file. (Splitting it into `VITE_API_BASE_URL` + `VITE_API_VERSION` is deferred — premature for a single version.)

---

## 9. Documentation updates

### 9.1 `README.md`

- Endpoint table: prepend `/api/v1` to all 7 entries.
- "Running the backend" note: Swagger URL stays `/swagger`, but mention the version dropdown surfaces `v1`.
- "API notes" line: state that the API uses URL-segment versioning, current version `v1`.

### 9.2 `CLAUDE.md`

- "Toolchain notes that bite" section: add a bullet noting `Asp.Versioning.Mvc 8.*` is pinned to the net8.0 line.
- "Architecture" section: add a two-sentence note that the API uses URL-segment versioning (`/api/v{version}/...`), configured in `Program.cs` via `AddApiVersioning(...)`, and that adding v2 means decorating new controllers with `[ApiVersion("2.0")]` — no further wiring needed.

### 9.3 Older specs

- `docs/superpowers/specs/2026-05-14-event-management-design.md` — leave as-is. This new spec amends it; both remain valid history.

---

## 10. Behavior matrix

| Request | Response |
|---|---|
| `GET /api/v1/events` | 200, list as before |
| `GET /api/v1/events/{id}` | 200/404 as before |
| `POST /api/v1/events` | 201 + Location `/api/v1/events/{id}` |
| `POST /api/v1/events/{eventId}/registrations` | 201 + Location `/api/v1/events/{eventId}/registrations/{id}` |
| `GET /api/events` (no version) | **404** (no route match) |
| `GET /api/v2/events` (unsupported version) | **400** with ProblemDetails ("Unsupported API version") |
| `GET /api/v1.0/events` | 200 (semver-style major-minor accepted by URL segment reader) |
| Response header `api-supported-versions: 1.0` | Present on every v1 response (from `ReportApiVersions = true`) |

---

## 11. Risk / compatibility notes

- **Single-process frontend is the only client.** No external integrations to migrate.
- **CORS unchanged.** Path prefix changes don't affect origin policy.
- **HttpLogging unchanged.** It logs Method/Path/StatusCode regardless of versioned routes.
- **Exception middleware unchanged.** Versioning lives in the routing layer above it.

---

## 12. Future improvements

- Move `v1` into an env var (`VITE_API_VERSION`) when a real v2 ships.
- Add `[Deprecated]` tag handling when a version is sunsetted.
- Consider per-endpoint versioning (`[MapToApiVersion("2.0")]`) if v2 changes only a subset of endpoints.
- Export `api-supported-versions` from frontend error handler so users see clearer messages on `400 Unsupported API version`.

---

## 13. Implementation order (informational)

The detailed task list will live in the writing-plans output. High-level order:

1. Add the two NuGet packages.
2. Add `ConfigureSwaggerOptions` class.
3. Update `Program.cs` (versioning registration, Swagger UI loop).
4. Update both controllers + the one hardcoded Location string.
5. Update existing integration test path strings.
6. Add `ApiVersioningTests`.
7. Run `dotnet test` — expect 21 passing.
8. Update frontend client `baseURL` and trim `/api/` from API module paths.
9. Run `npm run build` — expect clean.
10. Update `README.md` and `CLAUDE.md`.
11. Two commits: one for the backend change (steps 1-7) and one for frontend + docs (steps 8-10). Keeps the commit history clear and lets each commit be reverted independently.
