# URI Path Versioning — Design Spec

- **Date:** 2026-05-15
- **Status:** Approved — ready for implementation planning
- **Author:** Mukul Saini (with brainstorming via Claude)
- **Builds on:** `docs/superpowers/specs/2026-05-14-event-management-design.md`

This spec captures the agreed design for introducing URI path versioning to `EventManagement.Api`. It supersedes an earlier reverted attempt that used the `Asp.Versioning.Mvc` library; that approach was judged over-engineered for a take-home exercise.

The v1 design spec remains valid for everything not amended here.

---

## 1. Goal

Move every existing endpoint behind a `/api/v1/...` URL prefix using only route-attribute changes, so:

- Future versions (`v2`, ...) can be added later on the same host without disturbing v1 routes or controllers.
- No third-party versioning library is introduced.
- `Program.cs`, dependency injection, Swagger configuration, and CORS remain untouched.

This is a forward-looking polish on a take-home submission. There is no real v2 today.

---

## 2. Scope

### 2.1 In scope

- Two `[Route(...)]` attribute changes (one per controller) to add the `v1` segment.
- One hardcoded Location-header string update in `RegistrationsController.Register`.
- Path-literal updates in the two API integration test files so existing tests target the new paths.
- Frontend axios `baseURL` updated to append `/api/v1`; per-endpoint paths shortened to drop the now-shared prefix.
- `README.md` and `CLAUDE.md` updated to reflect the new endpoint paths.

### 2.2 Out of scope

- A second API version (v2, etc.). No behavior changes — only path versioning.
- A backward-compatibility alias for unversioned paths. The hard cut is intentional: the only client is the in-repo frontend, which is updated in lockstep.
- The `Asp.Versioning.*` packages, version readers, version dropdowns, and dynamic Swagger doc generation. Explicitly rejected as overkill for this exercise.
- A new `ApiVersioningTests` class. MVC routing returns 404 for unmatched paths without any code from us; an explicit test asserts nothing new.
- Any change to `Program.cs`, DI registration, middleware, services, repositories, domain rules, or DTOs.
- Frontend behavior changes (no new pages, no new mutations, no copy changes).
- Deprecation headers, sunset headers, or per-action versioning.

### 2.3 Decisions taken during brainstorming

- **No library.** A library (`Asp.Versioning.Mvc`) was considered and rejected. With a single version and a single in-repo client, hardcoding `v1` into two route attributes is dramatically simpler and equally correct.
- **Hard cut on unversioned routes.** `/api/events` returns 404 after the change. The only existing client is the in-repo frontend; preserving the old paths as aliases would double the surface in Swagger and tests with no benefit.
- **Version literal lives in axios `baseURL`, not per-endpoint paths.** Future v2 migration becomes a one-line change in `api/client.ts` instead of seven path-string edits.
- **No new tests for the versioning itself.** The existing 19 integration tests, after their path literals are updated, verify that the new paths work. ASP.NET Core routing returns 404 on unmatched routes natively; an extra "unversioned URL → 404" test would only assert framework behavior.

---

## 3. Backend changes

### 3.1 `EventsController.cs`

Only the route attribute changes; the controller body is untouched.

```csharp
[ApiController]
[Route("api/v1/events")]   // was: [Route("api/events")]
public sealed class EventsController : ControllerBase
{
    // body unchanged
}
```

### 3.2 `RegistrationsController.cs`

Route attribute and the hardcoded Location string in `Register`.

```csharp
[ApiController]
[Route("api/v1/events/{eventId:guid}/registrations")]   // was without v1
public sealed class RegistrationsController : ControllerBase
{
    // ...

    [HttpPost]
    public async Task<IActionResult> Register(Guid eventId, [FromBody] RegisterUserRequest req)
    {
        var input = new RegisterUserInput(req.UserId, req.UserName);
        var created = await _registrations.RegisterAsync(eventId, input);
        return Created(
            $"/api/v1/events/{eventId}/registrations/{created.Id}",   // was without v1
            created);
    }

    // other actions unchanged
}
```

### 3.3 `Program.cs`

**No changes.** The existing `builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", ...))` continues to work: `"v1"` is the internal Swagger document identifier and is unrelated to the URL prefix. The Swagger UI will pick up the new `/api/v1/...` routes automatically because that is what the controllers declare. CORS, HTTP logging, exception middleware, and DI remain identical.

---

## 4. Test changes

Mechanical path-literal updates only — no new test files, no new assertions.

### 4.1 `tests/EventManagement.Api.Tests/EventsControllerTests.cs`

Three path literals to update from `/api/events` to `/api/v1/events`:

- `POST_events_with_valid_body_returns_201_with_location_header` — `client.PostAsJsonAsync("/api/events", ...)`
- `GET_events_unknown_id_returns_404` — `client.GetAsync($"/api/events/{Guid.NewGuid()}")`
- `POST_events_with_empty_title_returns_400` — `client.PostAsJsonAsync("/api/events", ...)`

### 4.2 `tests/EventManagement.Api.Tests/RegistrationsControllerTests.cs`

Seven path literals to update:

- `CreateEvent` helper — `/api/events` → `/api/v1/events`
- `POST_registration_for_future_event_returns_201` — one `/api/events/{eventId}/registrations` literal
- `POST_duplicate_registration_returns_422` — two literals (the seed and the test call)
- `POST_registration_when_at_capacity_returns_422` — two literals
- `POST_registration_for_past_event_returns_422` — one literal

All other test code (assertions, helpers, factory setup, `FixedClock` usage) is unchanged.

### 4.3 Domain and Application tests

`EventManagement.Domain.Tests` and `EventManagement.Application.Tests` are unaffected — they do not reference HTTP paths.

---

## 5. Frontend changes

The version literal `v1` lives in exactly one place on the frontend: the axios `baseURL`.

### 5.1 `src/EventManagement.Web/src/api/client.ts`

```ts
import axios from "axios";

export const apiClient = axios.create({
  baseURL: (import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5050") + "/api/v1",
  headers: { "Content-Type": "application/json" },
});
```

`.env.development` is unchanged: `VITE_API_BASE_URL=http://localhost:5050`.

### 5.2 `src/EventManagement.Web/src/api/eventsApi.ts`

Drop the `/api` prefix from every path — it is now part of `baseURL`.

```ts
apiClient.get<EventSummary[]>("/events");
apiClient.get<EventDetail>(`/events/${id}`);
apiClient.post<EventSummary>("/events", payload);
apiClient.put<EventSummary>(`/events/${id}`, payload);
```

### 5.3 `src/EventManagement.Web/src/api/registrationsApi.ts`

Same pattern:

```ts
apiClient.get<Registration[]>(`/events/${eventId}/registrations`);
apiClient.post<Registration>(`/events/${eventId}/registrations`, payload);
apiClient.delete(`/events/${eventId}/registrations/${registrationId}`);
```

No other frontend files change. Components, hooks, query keys, types, and routes are unaffected.

---

## 6. Documentation changes

### 6.1 `README.md`

Update the endpoint table so every route has the `/v1` segment:

| Method | Route |
|---|---|
| `GET` | `/api/v1/events` |
| `GET` | `/api/v1/events/{id}` |
| `POST` | `/api/v1/events` |
| `PUT` | `/api/v1/events/{id}` |
| `GET` | `/api/v1/events/{eventId}/registrations` |
| `POST` | `/api/v1/events/{eventId}/registrations` |
| `DELETE` | `/api/v1/events/{eventId}/registrations/{registrationId}` |

Add a one-line note under the table: "API uses URI path versioning. Future versions will live alongside v1 under `/api/v2/...`, `/api/v3/...`, etc."

### 6.2 `CLAUDE.md`

One concrete path reference exists today: the "What this is" paragraph mentions `DELETE /api/events/{id}` as an example of a design choice. Update to `DELETE /api/v1/events/{id}`. No other path literals appear in the file.

---

## 7. Verification

Manual and automated checks at the end of implementation:

1. `dotnet build` — all 7 projects compile.
2. `dotnet test` — all 19 tests pass.
3. `dotnet run --project src/EventManagement.Api`, then:
   - `GET http://localhost:5050/api/v1/events` returns `200`.
   - `GET http://localhost:5050/api/events` returns `404`.
   - Swagger UI at `http://localhost:5050/swagger` shows the new `/api/v1/...` routes.
4. In a second terminal, `cd src/EventManagement.Web && npm run dev`, then exercise: list events, create an event, open the detail page, register a user, unregister a user. All flows succeed.

---

## 8. Why this design is right-sized

The previous attempt at this work added a versioning library, a dynamic-Swagger configuration class, a new test class, and over 900 lines of spec + plan. For a take-home with one version, one frontend, and seven endpoints, that machinery had no payoff.

This design accepts one trade-off in exchange for simplicity: adding v2 later will require the same kind of small, mechanical change (a second controller class or a second route attribute on a new endpoint set) rather than a configuration-driven branch on `ApiVersion`. That cost is paid only if and when v2 actually exists. Until then, the codebase carries the minimum machinery that satisfies the requirement.
