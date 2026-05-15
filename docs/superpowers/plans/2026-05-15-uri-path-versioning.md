# URI Path Versioning Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Move every existing endpoint behind a `/api/v1/...` URL prefix using only route-attribute changes, with no third-party versioning library.

**Architecture:** Two `[Route(...)]` attribute updates on the existing controllers, one hardcoded Location-string fix, mechanical path-literal updates in the API integration tests, a one-line frontend axios `baseURL` change, and docs updates. No new packages. No `Program.cs` changes. No new test classes. Hard cut: unversioned `/api/events` paths return 404.

**Tech Stack:** ASP.NET Core 8, xUnit + FluentAssertions, React 19 + Vite + axios.

**Spec:** `docs/superpowers/specs/2026-05-15-uri-path-versioning-design.md`

---

## Files touched

| File | Action | What changes |
|---|---|---|
| `src/EventManagement.Api/Controllers/EventsController.cs` | Modify line 9 | Route attribute: `api/events` → `api/v1/events` |
| `src/EventManagement.Api/Controllers/RegistrationsController.cs` | Modify lines 9, 25 | Route attribute + Location string add `/v1` |
| `tests/EventManagement.Api.Tests/EventsControllerTests.cs` | Modify lines 27, 40, 57 | Path literals: 3 occurrences |
| `tests/EventManagement.Api.Tests/RegistrationsControllerTests.cs` | Modify lines 24, 37, 51, 53, 65, 67, 83 | Path literals: 7 occurrences |
| `src/EventManagement.Web/src/api/client.ts` | Modify line 4 | Append `/api/v1` to `baseURL` |
| `src/EventManagement.Web/src/api/eventsApi.ts` | Modify lines 13, 17, 21, 25 | Drop `/api` prefix from path strings (4 occurrences) |
| `src/EventManagement.Web/src/api/registrationsApi.ts` | Modify lines 6, 10, 14 | Drop `/api` prefix from path strings (3 occurrences) |
| `README.md` | Modify lines 53–61 (+ insert note) | Endpoint table + one-line versioning note |
| `CLAUDE.md` | Modify line 7 | `DELETE /api/events/{id}` → `DELETE /api/v1/events/{id}` |

`Program.cs`, `EventManagement.Api.csproj`, DI extensions, services, repositories, domain code, frontend components/hooks/pages, and `.env.development` are **not** touched.

---

## Task 1: Version backend routes and update API integration tests

This task makes all backend changes in a single commit so the repo is green at every checkpoint. Updating the controllers without the tests, or vice versa, leaves the build failing — they must move together.

**Files:**
- Modify: `src/EventManagement.Api/Controllers/EventsController.cs` (line 9)
- Modify: `src/EventManagement.Api/Controllers/RegistrationsController.cs` (lines 9, 25)
- Modify: `tests/EventManagement.Api.Tests/EventsControllerTests.cs` (lines 27, 40, 57)
- Modify: `tests/EventManagement.Api.Tests/RegistrationsControllerTests.cs` (lines 24, 37, 51, 53, 65, 67, 83)

- [ ] **Step 1: Update path literals in `EventsControllerTests.cs`**

There are 3 occurrences of `/api/events`. Replace each with `/api/v1/events`:

```csharp
// Line 27 (in POST_events_with_valid_body_returns_201_with_location_header):
var response = await client.PostAsJsonAsync("/api/v1/events", ValidCreateBody());

// Line 40 (in GET_events_unknown_id_returns_404):
var response = await client.GetAsync($"/api/v1/events/{Guid.NewGuid()}");

// Line 57 (in POST_events_with_empty_title_returns_400):
var response = await client.PostAsJsonAsync("/api/v1/events", body);
```

- [ ] **Step 2: Update path literals in `RegistrationsControllerTests.cs`**

There are 7 occurrences. Replace each `/api/events` with `/api/v1/events`:

```csharp
// Line 24 (in CreateEvent helper):
var response = await client.PostAsJsonAsync("/api/v1/events", CreateEventBody(date, capacity));

// Line 37 (in POST_registration_for_future_event_returns_201):
var response = await client.PostAsJsonAsync($"/api/v1/events/{eventId}/registrations",
    RegisterBody("user-1", "Alice"));

// Line 51 (in POST_duplicate_registration_returns_422, seed call):
await client.PostAsJsonAsync($"/api/v1/events/{eventId}/registrations", RegisterBody("user-1", "Alice"));

// Line 53 (in POST_duplicate_registration_returns_422, assertion call):
var response = await client.PostAsJsonAsync($"/api/v1/events/{eventId}/registrations",
    RegisterBody("user-1", "Alice"));

// Line 65 (in POST_registration_when_at_capacity_returns_422, seed call):
await client.PostAsJsonAsync($"/api/v1/events/{eventId}/registrations", RegisterBody("user-1", "Alice"));

// Line 67 (in POST_registration_when_at_capacity_returns_422, assertion call):
var response = await client.PostAsJsonAsync($"/api/v1/events/{eventId}/registrations",
    RegisterBody("user-2", "Bob"));

// Line 83 (in POST_registration_for_past_event_returns_422):
var response = await client.PostAsJsonAsync($"/api/v1/events/{eventId}/registrations",
    RegisterBody("user-1", "Alice"));
```

- [ ] **Step 3: Run the API integration tests to confirm they fail (red)**

Run: `dotnet test tests/EventManagement.Api.Tests`

Expected: 9 of the 10 tests in `EventsControllerTests` + `RegistrationsControllerTests` fail, all returning `404 Not Found` instead of the expected `2xx` because the controllers still respond on the unversioned routes. The 10th test, `GET_events_unknown_id_returns_404`, accidentally passes because both the old and new paths produce `404` (the old via "controller matched but id not found", the new via "no controller matches"). This transient state is corrected in the next steps.

`EventManagement.Domain.Tests` and `EventManagement.Application.Tests` are unaffected and should still pass.

- [ ] **Step 4: Update `EventsController.cs` route attribute**

In `src/EventManagement.Api/Controllers/EventsController.cs`, change line 9:

```csharp
[ApiController]
[Route("api/v1/events")]   // was: [Route("api/events")]
public sealed class EventsController : ControllerBase
{
    // body unchanged
}
```

- [ ] **Step 5: Update `RegistrationsController.cs` route attribute and Location string**

In `src/EventManagement.Api/Controllers/RegistrationsController.cs`:

```csharp
// Line 9:
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
        // Line 25:
        return Created(
            $"/api/v1/events/{eventId}/registrations/{created.Id}",   // was without /v1
            created);
    }

    // other actions unchanged
}
```

- [ ] **Step 6: Run all tests and confirm green**

Run: `dotnet test`

Expected: all 19 tests pass across the three test projects. If any fail, re-read steps 1–5 — a missed path literal or a typo in a route attribute is the most likely cause.

- [ ] **Step 7: Manual smoke — verify hard cut**

Start the API: `dotnet run --project src/EventManagement.Api`

In a second terminal:

```bash
curl -i http://localhost:5050/api/v1/events
# Expected: HTTP/1.1 200 OK with JSON body []

curl -i http://localhost:5050/api/events
# Expected: HTTP/1.1 404 Not Found
```

Stop the API (Ctrl+C).

- [ ] **Step 8: Commit**

```bash
git add src/EventManagement.Api/Controllers/EventsController.cs `
        src/EventManagement.Api/Controllers/RegistrationsController.cs `
        tests/EventManagement.Api.Tests/EventsControllerTests.cs `
        tests/EventManagement.Api.Tests/RegistrationsControllerTests.cs
git commit -m "feat(api): version routes under /api/v1"
```

Commit body should describe the hard cut: unversioned `/api/events` paths now return 404 and the only client (the in-repo frontend) is updated in the next task.

---

## Task 2: Point the frontend axios client at /api/v1

The version literal `v1` lives in exactly one place on the frontend: the `baseURL` in `client.ts`. Per-endpoint paths drop their `/api` prefix accordingly.

Between this task's start and end, the frontend dev experience is temporarily broken (the previous commit pointed the API at `/api/v1/...` but the frontend still calls `/api/events`). That's expected and contained within this task.

**Files:**
- Modify: `src/EventManagement.Web/src/api/client.ts` (line 4)
- Modify: `src/EventManagement.Web/src/api/eventsApi.ts` (lines 13, 17, 21, 25)
- Modify: `src/EventManagement.Web/src/api/registrationsApi.ts` (lines 6, 10, 14)

- [ ] **Step 1: Update `client.ts` to append `/api/v1` to `baseURL`**

```ts
import axios from "axios";

export const apiClient = axios.create({
  baseURL: (import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5050") + "/api/v1",
  headers: { "Content-Type": "application/json" },
});
```

The fallback string remains a host-only URL; the `/api/v1` suffix is added in code so both the env-var and fallback paths get the version automatically. `.env.development` is **not** changed.

- [ ] **Step 2: Update `eventsApi.ts` — drop `/api` from every path**

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
    const { data } = await apiClient.get<EventSummary[]>("/events");
    return data;
  },
  get: async (id: string): Promise<EventDetail> => {
    const { data } = await apiClient.get<EventDetail>(`/events/${id}`);
    return data;
  },
  create: async (payload: CreateEventPayload): Promise<EventSummary> => {
    const { data } = await apiClient.post<EventSummary>("/events", payload);
    return data;
  },
  update: async (id: string, payload: CreateEventPayload): Promise<EventSummary> => {
    const { data } = await apiClient.put<EventSummary>(`/events/${id}`, payload);
    return data;
  },
};
```

- [ ] **Step 3: Update `registrationsApi.ts` — drop `/api` from every path**

```ts
import { apiClient } from "./client";
import type { Registration, RegisterUserPayload } from "../features/registrations/types/registration";

export const registrationsApi = {
  list: async (eventId: string): Promise<Registration[]> => {
    const { data } = await apiClient.get<Registration[]>(`/events/${eventId}/registrations`);
    return data;
  },
  register: async (eventId: string, payload: RegisterUserPayload): Promise<Registration> => {
    const { data } = await apiClient.post<Registration>(`/events/${eventId}/registrations`, payload);
    return data;
  },
  unregister: async (eventId: string, registrationId: string): Promise<void> => {
    await apiClient.delete(`/events/${eventId}/registrations/${registrationId}`);
  },
};
```

- [ ] **Step 4: Type-check and lint the frontend**

```bash
cd src/EventManagement.Web
npm run build
npm run lint
```

Expected: both succeed with no errors. `npm run build` runs `tsc -b && vite build`; type errors here typically mean a path-string typo (e.g. `/event/` instead of `/events/`).

- [ ] **Step 5: Manual smoke — exercise the UI end-to-end**

In one terminal: `dotnet run --project src/EventManagement.Api`
In another (from `src/EventManagement.Web`): `npm run dev`

Open `http://localhost:5173` and verify every flow:

1. **List page** loads with an empty list (no error toast).
2. **Create event** — fill the form, submit, see the new event appear in the list.
3. **Event detail** — click the event, see its details and an empty registrations list.
4. **Register** — submit the registration form, see the new registration appear with no error.
5. **Unregister** — confirm the modal, see the registration disappear.
6. **Inline edit on detail page** — change the title, save, see the update reflected.

Open browser devtools → Network tab while clicking around. Confirm every request goes to `http://localhost:5050/api/v1/...` and returns `2xx`. No request should hit `/api/events` without the `/v1`.

Stop both processes.

- [ ] **Step 6: Commit**

```bash
git add src/EventManagement.Web/src/api/client.ts `
        src/EventManagement.Web/src/api/eventsApi.ts `
        src/EventManagement.Web/src/api/registrationsApi.ts
git commit -m "feat(web): point axios client at /api/v1"
```

---

## Task 3: Update README and CLAUDE.md

Reflect the new routes in the two docs that mention them. The 2026-05-14 spec/plan documents themselves are historical and should not be edited — they describe v1 of the implementation before path versioning was added.

**Files:**
- Modify: `README.md` (lines 53–61, plus insert one note line)
- Modify: `CLAUDE.md` (line 7)

- [ ] **Step 1: Update the endpoint table in `README.md`**

Replace lines 53–61 (the entire `## API endpoints` table) with the versioned routes, and add a one-line note immediately under the table:

```markdown
## API endpoints

| Method | Route | Purpose |
|---|---|---|
| GET | `/api/v1/events` | List events |
| GET | `/api/v1/events/{id}` | Get event by id |
| POST | `/api/v1/events` | Create event |
| PUT | `/api/v1/events/{id}` | Update event |
| GET | `/api/v1/events/{eventId}/registrations` | List registrations for an event |
| POST | `/api/v1/events/{eventId}/registrations` | Register a user |
| DELETE | `/api/v1/events/{eventId}/registrations/{registrationId}` | Unregister a user |

API uses URI path versioning. Future versions will live alongside v1 under `/api/v2/...`, `/api/v3/...`, etc.

Response codes: 200 (read/update), 201 (create/register), 204 (delete), 400 (DataAnnotations validation), 404 (not found), 422 (business rule violation), 500 (unexpected). 400 responses use ASP.NET Core's `ValidationProblemDetails`; other errors use a small `{ message, statusCode, details }` shape.
```

(The "Response codes" paragraph already exists at line 63 — keep it as-is. Only the table and the new note line are new.)

- [ ] **Step 2: Update `CLAUDE.md`**

In `CLAUDE.md` line 7, the "What this is" paragraph contains the phrase `why there is no DELETE /api/events/{id}`. Change it to `why there is no DELETE /api/v1/events/{id}`.

No other path literals appear in `CLAUDE.md` — `Grep` for `/api/events` should return only this one match.

- [ ] **Step 3: Verify the doc changes**

```bash
git diff README.md CLAUDE.md
```

Skim the diff: every old `/api/events` reference in these two files should now be `/api/v1/events`, and the new versioning note should appear under the README endpoint table.

- [ ] **Step 4: Commit**

```bash
git add README.md CLAUDE.md
git commit -m "docs: update endpoint references for /api/v1 path versioning"
```

---

## Final verification

After all three tasks are committed, run a final integrated check:

- [ ] **Step 1: Full test suite**

```bash
dotnet test
```

Expected: 19 tests pass.

- [ ] **Step 2: Frontend build**

```bash
cd src/EventManagement.Web
npm run build
```

Expected: build succeeds with no errors.

- [ ] **Step 3: End-to-end smoke**

Start the API (`dotnet run --project src/EventManagement.Api`) and the frontend (`npm run dev` in `src/EventManagement.Web`). Open `http://localhost:5173` and exercise:

- list events
- create an event
- open detail page
- register a user
- unregister a user
- inline-edit the event

All flows should succeed with requests going to `/api/v1/...`. Hit `http://localhost:5050/api/events` directly in a browser or via curl and confirm `404 Not Found`. Open Swagger UI at `http://localhost:5050/swagger` and confirm the listed routes are the versioned ones.

- [ ] **Step 4: Git log review**

```bash
git log --oneline -5
```

Expected: three new commits on top of `b754ea5` (the design spec commit), in this order:

```
<sha> docs: update endpoint references for /api/v1 path versioning
<sha> feat(web): point axios client at /api/v1
<sha> feat(api): version routes under /api/v1
b754ea5 docs: add design spec for URI path versioning
```
