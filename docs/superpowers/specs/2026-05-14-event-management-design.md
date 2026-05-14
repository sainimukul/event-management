# Event Management — Design Spec

- **Date:** 2026-05-14
- **Status:** Approved — ready for implementation planning
- **Author:** Mukul Saini (with brainstorming via Claude)
- **Source requirements:** `docs/requirements/event-management-requirements-v1.md`
- **Earlier draft:** `docs/requirements/submission_ready_implementation_plan_v2.md`

This spec is the single source of truth for the implementation. It supersedes the v2 implementation plan where they differ. Decisions in this document have been deliberately chosen during a brainstorming pass; rationale is captured for the non-obvious ones.

---

## 1. Goal

Build a take-home submission consisting of:

- An ASP.NET Core REST API for managing **events** and attendee **registrations**.
- A React + Vite + TypeScript SPA for using the API.
- Unit tests covering the business rules and service-layer behavior.
- A README explaining how to run the system and what assumptions were made.

The submission must satisfy the v1 requirements while demonstrating engineering judgment — right-sized structure, clear boundaries, and only the dependencies the problem earns.

---

## 2. Scope

### 2.1 In scope

- CRU operations on events (Create, Read, Update — no Delete; see §2.3)
- Register and unregister attendees for an event; list an event's registrations
- Enforce business rules:
  - cannot register for past events
  - cannot exceed event max capacity
  - cannot double-register the same user for the same event
- In-memory storage (state resets on restart)
- Unit tests for business rules and service behavior
- API integration tests for the controllers
- React SPA with three pages (list, create, detail with inline edit + registrations)
- README with run instructions and explicit assumptions

### 2.2 Time budget

~4 hours total. Scope cuts taken to fit (see §2.4).

### 2.3 Out of scope

- **DELETE event endpoint.** v1 §3 lists only Create/Read/Update for events. v1 §5 says "Standard CRUD" but is contradicted by §3; the strict reading of §3 wins. Documented in the README assumptions.
- Authentication / authorization
- Persistence to disk or DB
- Pagination, waitlists, soft delete, audit history, notifications, recurring events
- Frontend automated tests (v1 requires tests for business logic only)
- Custom request-logging middleware (use built-in `UseHttpLogging`)
- Dedicated Edit route on the frontend (edit is inline on the Detail page)
- Serilog, FluentValidation, Tailwind CSS (built-in / plain alternatives chosen)

### 2.4 Scope cuts taken (during brainstorming, to fit 4h budget)

- Merge Edit into Detail page (one `<EventForm>` reused for Create and inline Edit)
- Skip custom request-logging middleware → `app.UseHttpLogging()`
- Skip frontend tests

---

## 3. Stack and key dependency choices

### 3.1 Backend

| Concern | Choice | Notes |
|---|---|---|
| Runtime | .NET 8 | Current LTS |
| Web framework | ASP.NET Core | v1 mandates C# / ASP.NET Core option |
| Logging | `Microsoft.Extensions.Logging` (built-in) | Serilog explicitly rejected — no extra dependency needed for the brief. |
| Request/response logging | `app.UseHttpLogging()` | Built-in; replaces custom middleware. |
| Validation | DataAnnotations | `[Required]`, `[StringLength]`, `[Range]` on request models. FluentValidation explicitly rejected — overkill for ~6 fields. |
| OpenAPI | Swashbuckle (Swagger UI) | Standard ASP.NET Core integration. |
| Tests | xUnit + FluentAssertions + NSubstitute | NSubstitute reads cleaner than Moq for repo mocks. |
| Integration tests | `WebApplicationFactory<Program>` | Built-in. |

### 3.2 Frontend

| Concern | Choice | Notes |
|---|---|---|
| Framework | React 18 + Vite + TypeScript | v1 lists React (Vite) as an option. |
| Routing | `react-router-dom` v6 | Standard SPA routing. |
| Server state | TanStack Query (`@tanstack/react-query`) | Removes per-page loading/error/data triplets; signals modern React judgment. |
| HTTP client | `axios` | Used inside TanStack Query fetchers. |
| Styling | Plain CSS via CSS Modules (`*.module.css` per component) | Tailwind explicitly rejected — no postcss/config overhead. One small global `index.css` for resets, color variables, and a handful of utility classes (`.card`, `.button`, `.button--danger`, `.form-row`). Aim for ~150 lines of total CSS. |
| Toast | `react-hot-toast` | ~3 KB; success/error toasts on mutations. |
| Date input | Native `<input type="datetime-local">` | Converted to ISO 8601 UTC before POST/PUT. |

### 3.3 Rationale for keeping these vs. the "use built-in" trend

- **TanStack Query** earns its keep — it removes meaningful boilerplate across the three pages and caches list/detail data so the Detail page is instant on navigation.
- **axios** and **react-hot-toast** are small. They could be replaced with `fetch` and inline banners if minimizing dependencies further is preferred; that's documented as a future cleanup option, not a blocker.

### 3.4 Ports and CORS

- Backend: ASP.NET Core dev profile uses `http://localhost:5050` (HTTP) — configured in `Properties/launchSettings.json` so Swagger lands at `http://localhost:5050/swagger`.
- Frontend: Vite default `http://localhost:5173`.
- CORS: API enables a named policy `AllowLocalDev` that whitelists `http://localhost:5173` and is applied **only when `app.Environment.IsDevelopment()` is true**. Production-style configuration is not needed because deployment is out of scope.
- Frontend reads the API base URL from `VITE_API_BASE_URL` (with a default of `http://localhost:5050` in `.env.development`).

---

## 4. Solution layout

Four backend projects + one frontend project + three test projects.

```
EventManagement/
├── src/
│   ├── EventManagement.Api/
│   ├── EventManagement.Application/
│   ├── EventManagement.Domain/
│   ├── EventManagement.Infrastructure/
│   └── EventManagement.Web/
├── tests/
│   ├── EventManagement.Domain.Tests/
│   ├── EventManagement.Application.Tests/
│   └── EventManagement.Api.Tests/
└── README.md
```

### 4.1 Project references

- `Api` → `Application` + `Infrastructure`
- `Application` → `Domain`
- `Infrastructure` → `Application` + `Domain`
- Test projects → their corresponding source project + the projects it references

### 4.2 Folder layout per project

**`EventManagement.Api/`**
```
Controllers/
  EventsController.cs
  RegistrationsController.cs
Middleware/
  ExceptionHandlingMiddleware.cs
Models/
  Events/
    CreateEventRequest.cs
    UpdateEventRequest.cs
  Registrations/
    RegisterUserRequest.cs
Program.cs
appsettings.json
```
*(No `RequestLoggingMiddleware.cs` — replaced by `UseHttpLogging()`.)*

**`EventManagement.Application/`**
```
Events/
  EventService.cs
  DTOs/
    EventDto.cs
    EventDetailDto.cs
Registrations/
  RegistrationService.cs
  DTOs/
    RegistrationDto.cs
Interfaces/
  IEventRepository.cs
  IRegistrationRepository.cs
  IClock.cs
```

**`EventManagement.Domain/`**
```
Entities/
  Event.cs
  Registration.cs
Exceptions/
  EventInPastException.cs
  EventCapacityExceededException.cs
  DuplicateRegistrationException.cs
  EventNotFoundException.cs
  RegistrationNotFoundException.cs
Services/
  RegistrationRules.cs
```

**`EventManagement.Infrastructure/`**
```
Persistence/
  InMemoryEventRepository.cs
  InMemoryRegistrationRepository.cs
Time/
  SystemClock.cs
DependencyInjection.cs
```

**`EventManagement.Web/src/`**
```
api/
  client.ts                # axios instance + base URL
  eventsApi.ts
  registrationsApi.ts
features/
  events/
    components/
      EventForm.tsx        # shared by Create and Detail-edit
      EventForm.module.css
      EventCard.tsx
      EventCard.module.css
    hooks/
      useEvents.ts         # TanStack Query hooks
      useEvent.ts
      useCreateEvent.ts
      useUpdateEvent.ts
    pages/
      EventListPage.tsx
      EventListPage.module.css
      CreateEventPage.tsx
      CreateEventPage.module.css
      EventDetailPage.tsx
      EventDetailPage.module.css
  registrations/
    components/
      RegistrationList.tsx
      RegistrationList.module.css
      RegisterForm.tsx
      RegisterForm.module.css
    hooks/
      useRegistrations.ts
      useRegister.ts
      useUnregister.ts
    types/
      registration.ts
shared/
  components/
    Spinner.tsx
    EmptyState.tsx
    ErrorBanner.tsx
    ConfirmModal.tsx
  utils/
    date.ts                # ISO/local conversion helpers
App.tsx                    # router setup, QueryClient provider, Toaster
main.tsx
index.css                  # global resets, vars, utility classes
```

---

## 5. Domain model

### 5.1 `Event` entity

| Property | Type | Notes |
|---|---|---|
| `Id` | `Guid` | Generated on creation |
| `Title` | `string` | Required |
| `Description` | `string?` | Optional |
| `Date` | `DateTimeOffset` | Stored as UTC |
| `MaxCapacity` | `int` | Must be > 0 |
| `CreatedAt` | `DateTimeOffset` | Set on creation (UTC) |
| `UpdatedAt` | `DateTimeOffset` | Set on update (UTC) |

### 5.2 `Registration` entity

| Property | Type | Notes |
|---|---|---|
| `Id` | `Guid` | Generated on creation |
| `EventId` | `Guid` | Foreign key (logical) |
| `UserId` | `string` | Required |
| `UserName` | `string` | Required |
| `RegisteredAt` | `DateTimeOffset` | Set on creation (UTC) |

### 5.3 Domain exceptions

All inherit from a `DomainException` base class so the exception middleware can pattern-match by base type if useful.

- `EventNotFoundException`
- `RegistrationNotFoundException`
- `EventInPastException`
- `EventCapacityExceededException`
- `DuplicateRegistrationException`

---

## 6. Business rules

`Domain/Services/RegistrationRules.cs` is a pure domain service. No HTTP, no logging, no repositories.

```text
EnsureEventIsNotInPast(Event ev, DateTimeOffset now)
EnsureCapacityAvailable(Event ev, int currentRegistrationCount)
EnsureUserNotAlreadyRegistered(string userId, IEnumerable<Registration> registrations)
```

`now` is passed in (not read from `DateTimeOffset.UtcNow` inside) so tests don't depend on wall-clock time. The clock is injected at the service boundary via `IClock`.

---

## 7. Concurrency

The check-count → check-duplicate → insert sequence for registrations must be atomic per event.

**Approach:** `RegistrationService` holds a `ConcurrentDictionary<Guid, SemaphoreSlim>`. The first registration to a given event lazily creates a `SemaphoreSlim(1, 1)`; subsequent registrations to the same event reuse it. The critical section runs inside `WaitAsync` / `Release`.

**Why per-event (not global):** registrations to different events do not need to serialize against each other.

**Why this is enough for the brief:** the app is single-process, in-memory, and demo-scale. Distributed locks, optimistic concurrency tokens, etc. are explicitly out of scope.

---

## 8. Repositories

### 8.1 Interfaces (in Application layer)

**`IEventRepository`**
- `Task<IReadOnlyList<Event>> GetAllAsync()`
- `Task<Event?> GetByIdAsync(Guid id)`
- `Task AddAsync(Event ev)`
- `Task UpdateAsync(Event ev)`

**`IRegistrationRepository`**
- `Task<IReadOnlyList<Registration>> GetByEventIdAsync(Guid eventId)`
- `Task<Registration?> GetByIdAsync(Guid id)`
- `Task AddAsync(Registration registration)`
- `Task DeleteAsync(Guid registrationId)`
- `Task<bool> ExistsAsync(Guid eventId, string userId)`
- `Task<int> CountByEventIdAsync(Guid eventId)`

`Async` everywhere even though in-memory is synchronous, so the seam to a real DB later is clean.

### 8.2 In-memory implementations (in Infrastructure layer)

- `ConcurrentDictionary<Guid, Event>` and `ConcurrentDictionary<Guid, Registration>`
- All methods return `Task.FromResult(...)`
- Registered as **singletons** in DI (state persists across requests, resets on restart)

### 8.3 `IClock`

```csharp
public interface IClock { DateTimeOffset UtcNow { get; } }
public sealed class SystemClock : IClock { public DateTimeOffset UtcNow => DateTimeOffset.UtcNow; }
```

Registered as a singleton. Tests inject a `FixedClock`.

---

## 9. Application services

### 9.1 `EventService`

Responsibilities:
- `CreateAsync(CreateEventInput)` → returns `EventDto`
- `GetByIdAsync(Guid)` → returns `EventDetailDto?`
- `GetAllAsync()` → returns `IReadOnlyList<EventDto>`
- `UpdateAsync(Guid, UpdateEventInput)` → returns `EventDto`; throws `EventNotFoundException` if missing

Validation it owns (beyond DataAnnotations at the request boundary):
- title non-empty (trim before check)
- max capacity > 0
- event exists before update

### 9.2 `RegistrationService`

Responsibilities:
- `RegisterAsync(Guid eventId, RegisterUserInput)` → returns `RegistrationDto`
- `UnregisterAsync(Guid eventId, Guid registrationId)` → throws `RegistrationNotFoundException` if missing
- `ListForEventAsync(Guid eventId)` → returns `IReadOnlyList<RegistrationDto>`

**Register flow:**
1. Load event; if not found → `EventNotFoundException`
2. Acquire per-event semaphore
3. Load registrations for event
4. Apply `RegistrationRules` (past, capacity, duplicate)
5. Create + save `Registration`
6. Release semaphore
7. Return DTO

**Unregister flow:**
1. Load registration; if not found → `RegistrationNotFoundException`
2. Delete registration

---

## 10. Request / response models

### 10.1 Request models (with DataAnnotations)

**`CreateEventRequest`** (and structurally identical `UpdateEventRequest`)
- `Title` — `[Required]`, `[StringLength(200, MinimumLength = 1)]`
- `Description` — `[StringLength(2000)]` (optional)
- `Date` — `[Required]`, `DateTimeOffset`
- `MaxCapacity` — `[Range(1, int.MaxValue)]`

**`RegisterUserRequest`**
- `UserId` — `[Required]`, `[StringLength(100, MinimumLength = 1)]`
- `UserName` — `[Required]`, `[StringLength(200, MinimumLength = 1)]`

Controllers are decorated with `[ApiController]`; ASP.NET Core auto-returns `400` with `ValidationProblemDetails` when model state is invalid.

### 10.2 DTOs

- `EventDto` — id, title, date, maxCapacity, currentRegistrations, createdAt
- `EventDetailDto` — `EventDto` + description + updatedAt
- `RegistrationDto` — id, eventId, userId, userName, registeredAt

Entities never leave the Application layer; controllers only see DTOs.

---

## 11. API contract

### 11.1 Endpoints

| Method | Route | Purpose | Success |
|---|---|---|---|
| `GET` | `/api/events` | List all events | 200 |
| `GET` | `/api/events/{id}` | Get event by id | 200 / 404 |
| `POST` | `/api/events` | Create event | 201 + Location |
| `PUT` | `/api/events/{id}` | Update event | 200 / 404 |
| `GET` | `/api/events/{eventId}/registrations` | List registrations | 200 |
| `POST` | `/api/events/{eventId}/registrations` | Register user | 201 + Location |
| `DELETE` | `/api/events/{eventId}/registrations/{registrationId}` | Unregister | 204 / 404 |

### 11.2 Status codes

| Scenario | Code |
|---|---|
| List/get/update success | 200 |
| Create/register success | 201 |
| Delete success | 204 |
| DataAnnotations validation failure | 400 (`ValidationProblemDetails`) |
| Resource not found | 404 |
| Business rule violation (past event, capacity, duplicate) | 422 |
| Unhandled exception | 500 |

### 11.3 Error payload (non-validation errors)

```json
{
  "message": "Event capacity exceeded",
  "statusCode": 422,
  "details": null
}
```

Validation failures use the standard ASP.NET Core `ValidationProblemDetails` shape (RFC 7807-ish) so Swagger and clients understand it.

---

## 12. Exception handling

`ExceptionHandlingMiddleware` catches exceptions thrown out of controllers and maps them:

| Exception | Status | Notes |
|---|---|---|
| `EventNotFoundException` | 404 | |
| `RegistrationNotFoundException` | 404 | |
| `EventInPastException` | 422 | |
| `EventCapacityExceededException` | 422 | |
| `DuplicateRegistrationException` | 422 | |
| (anything else) | 500 | Logged at `Error` level with stack trace |

Validation errors are handled by the framework (`[ApiController]`) before reaching the middleware.

---

## 13. Logging

- `Microsoft.Extensions.Logging` only — no Serilog
- `appsettings.json` sets `Logging:LogLevel:Default` to `Information`, `Microsoft.AspNetCore` to `Warning`
- `app.UseHttpLogging()` configured to log Method, Path, StatusCode, and duration
- Business rule violations (caught in exception middleware) → `Warning`
- Unhandled exceptions → `Error` with stack trace
- Structured properties: include `EventId` / `UserId` / `RegistrationId` where relevant via `BeginScope` or message templates

No logging for every internal method call. Console sink is the default ASP.NET Core console formatter.

---

## 14. Frontend pages and UX

### 14.1 Routes

- `/` → `EventListPage`
- `/events/new` → `CreateEventPage`
- `/events/:id` → `EventDetailPage` (shows event details, registrations, register form, inline edit affordance)

### 14.2 Page responsibilities

**EventListPage**
- Fetch `['events']` via `useEvents`
- Render a list of `EventCard`s (title, date in local time, capacity, current count)
- Each card links to `/events/:id`
- Top-right action: "New Event" → `/events/new`
- States: loading spinner; empty state ("No events yet — create one"); error banner with retry

**CreateEventPage**
- Renders `<EventForm mode="create" />`
- On success: navigate to `/events/:id`, show success toast
- On failure: surface server `ValidationProblemDetails` field errors inline

**EventDetailPage**
- Fetch `['events', id]` and `['events', id, 'registrations']`
- Shows event details
- "Edit" button toggles inline `<EventForm mode="edit" initialData={event} />`
- Below: `RegistrationList` + `RegisterForm`
- Unregister: `ConfirmModal` then `useUnregister` mutation
- States: loading, error banner with retry, success toasts on register/unregister/update

### 14.3 Shared `<EventForm>`

Props: `mode: 'create' | 'edit'`, `initialData?: EventDto`, `onSuccess?: () => void`.
Fields: title (text), description (textarea), date (`datetime-local`), maxCapacity (number ≥ 1).
Submit:
- Convert `datetime-local` value to ISO 8601 UTC before POST/PUT
- On success: invalidate relevant query keys, show success toast, call `onSuccess`
- On 400: parse `ValidationProblemDetails.errors` and render field-level errors
- On 422: show error banner with the business-rule message

### 14.4 Query keys and invalidation

- `['events']` — list
- `['events', id]` — detail
- `['events', id, 'registrations']` — registrations list

| Mutation | Invalidates |
|---|---|
| Create event | `['events']` |
| Update event | `['events']`, `['events', id]` |
| Register | `['events', id]` (current count changes), `['events', id, 'registrations']` |
| Unregister | `['events', id]`, `['events', id, 'registrations']` |

### 14.5 Styling

CSS Modules per component. Global `index.css`:
- CSS reset
- Color variables: `--color-bg`, `--color-surface`, `--color-text`, `--color-muted`, `--color-primary`, `--color-danger`
- Typography (system font stack)
- Utility classes: `.card`, `.button`, `.button--primary`, `.button--danger`, `.button--ghost`, `.form-row`, `.input`, `.textarea`, `.label`

Target ~150 lines of CSS total. Layout is single-column, max-width ~720px, centered.

### 14.6 Date handling

- Backend stores `DateTimeOffset` UTC
- Frontend sends ISO 8601 UTC strings (e.g. `2026-06-01T18:30:00Z`)
- Frontend displays in user's local timezone via `Intl.DateTimeFormat`
- `<input type="datetime-local">` round-trip helpers live in `shared/utils/date.ts`

---

## 15. Testing

### 15.1 Domain tests — `EventManagement.Domain.Tests`

`RegistrationRulesTests` — four cases (one per rule + the success path):
1. Registration succeeds for a future event with available capacity and a new user.
2. `EnsureEventIsNotInPast` throws `EventInPastException` when `event.Date < now`.
3. `EnsureCapacityAvailable` throws `EventCapacityExceededException` when `currentCount == MaxCapacity`.
4. `EnsureUserNotAlreadyRegistered` throws `DuplicateRegistrationException` when the user has an existing registration.

### 15.2 Application tests — `EventManagement.Application.Tests`

`RegistrationServiceTests` (NSubstitute mocks for both repositories + `FixedClock`):
1. Register: success path
2. Register: throws `EventNotFoundException` when event missing
3. Register: throws `DuplicateRegistrationException` when user already registered
4. Register: throws `EventCapacityExceededException` when full
5. Unregister: success path

`EventServiceTests`:
1. Create: returns DTO with generated id and timestamps
2. Update: throws `EventNotFoundException` if not found
3. GetById: returns DTO

### 15.3 Integration tests — `EventManagement.Api.Tests`

Using `WebApplicationFactory<Program>` with a per-test DI override that swaps the singleton repos so tests don't share state. Optionally a `FixedClock` for past-event tests.

`EventsControllerTests`:
1. `POST /api/events` with valid body returns 201 + Location header
2. `GET /api/events/{id}` with unknown id returns 404
3. `POST /api/events` with empty title returns 400 with `ValidationProblemDetails`

`RegistrationsControllerTests`:
1. `POST /api/events/{id}/registrations` valid → 201
2. Duplicate registration → 422 with `DuplicateRegistrationException` message
3. Capacity full → 422
4. Past event → 422 (uses `FixedClock` set to after event date)

### 15.4 Frontend tests

None. Out of scope.

---

## 16. README contents

The README at the repo root must contain:

1. **Overview** — one paragraph on what the app does
2. **Tech stack** — ASP.NET Core 8, React + Vite + TypeScript, in-memory storage, xUnit
3. **How to run the backend** — `dotnet run --project src/EventManagement.Api` (defaults to `http://localhost:5050`; Swagger at `/swagger`)
4. **How to run the frontend** — `npm install && npm run dev` from `src/EventManagement.Web` (defaults to `http://localhost:5173`; reads API base URL from `VITE_API_BASE_URL`)
5. **How to run tests** — `dotnet test`
6. **API notes** — base URL (`http://localhost:5050`), Swagger URL (`/swagger`), endpoint summary, CORS policy `AllowLocalDev` whitelists the Vite dev server in Development only
7. **Assumptions** (see §17)
8. **Future improvements** — physical DB, pagination, auth, Docker, Serilog/FluentValidation if rules grow

---

## 17. Documented assumptions

The README must explicitly state:

1. **DELETE event endpoint is intentionally omitted.** v1 §3 lists only Create/Read/Update for events. v1 §5 says "Standard CRUD" but is contradicted by §3; the strict reading of §3 wins.
2. **`userId` and `userName` come from the request body** because authentication is out of scope per v1 §7.
3. **State resets on process restart.** In-memory storage per v1 §4.
4. **All dates are stored as UTC and displayed in the user's local timezone.**
5. **Logging via built-in `Microsoft.Extensions.Logging`.** Serilog would be the natural next step for production.
6. **Validation via DataAnnotations.** FluentValidation would be the natural next step if validation rules grow complex.
7. **Per-event in-process lock around the registration flow.** Sufficient for single-process in-memory; would migrate to optimistic concurrency or a DB-level constraint if persisted.

---

## 18. Manual verification checklist

Before submission, manually verify via Swagger and the UI:

- Create an event (valid + invalid bodies → 201 / 400)
- List events
- Get event detail (valid + unknown id → 200 / 404)
- Update event (valid + unknown id → 200 / 404)
- Register a user for a future event (success → 201)
- Register the same user again → 422
- Register beyond capacity → 422
- Register for a past event → 422
- Unregister a user (valid + unknown id → 204 / 404)
- All FE pages render loading, empty, error states correctly
- Toasts fire on success
- Confirm modal blocks accidental unregisters

---

## 19. Quality bar

The final submission should demonstrate:

- Clear layer boundaries (Api / Application / Domain / Infrastructure)
- Centralized, testable business rules
- Right-sized dependency choices (no Serilog/FluentValidation/Tailwind dragged in unnecessarily)
- Thread-safe registration under load
- Consistent error contract (404 / 422 / 500 + `ValidationProblemDetails` for 400)
- Useful, non-noisy logs
- Meaningful tests focused on what v1 actually mandates
- A simple but clean UI that handles loading / empty / error states
- A README that is honest about assumptions

---

## 20. Implementation order (informational)

Detailed implementation steps will live in the writing-plans output. High-level order:

1. Backend scaffold (4 projects + refs + DI + Swagger + logging)
2. Domain layer + `RegistrationRules` tests
3. Application layer (interfaces, DTOs, services) + service tests
4. Infrastructure layer (in-memory repos + `SystemClock`)
5. API layer (controllers, request models with DataAnnotations, exception middleware)
6. API integration tests
7. Frontend scaffold (Vite + CSS Modules + TanStack Query + react-router)
8. Frontend pages (List → Create → Detail with edit + registrations)
9. README + manual verification
