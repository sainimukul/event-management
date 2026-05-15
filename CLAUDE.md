# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A take-home Event Management exercise: ASP.NET Core 8 REST API + React 19 SPA, in-memory storage, no auth, no persistence. The brief is `docs/requirements/event-management-requirements-v1.md`. The final design lives in `docs/superpowers/specs/2026-05-14-event-management-design.md` and the task-by-task plan in `docs/superpowers/plans/2026-05-14-event-management.md` — read those first when you need to understand *why* something is shaped a certain way (e.g. why there is no `DELETE /api/v1/events/{id}`, why we use `Microsoft.Extensions.Logging` instead of Serilog, why CSS Modules instead of Tailwind).

## Toolchain notes that bite

- **.NET 10 SDK builds `net8.0` targets.** All csprojs pin `<TargetFramework>net8.0</TargetFramework>`. Do not regenerate them.
- **The solution file is `EventManagement.sln` (classic format), not `.slnx`.** .NET 10's `dotnet new sln` defaults to `.slnx`, which .NET 8 SDK cannot read. If you ever recreate the solution, pass `--format sln`. See commit `8dca308`.
- **`Microsoft.AspNetCore.Mvc.Testing` is pinned to `8.*`** in `tests/EventManagement.Api.Tests` — newer versions require `net10.0`.
- **Vite 8 + React 19 + react-router-dom v7** despite spec language mentioning React 18 / router v6 (the spec was drafted before the scaffold pulled latest).

## Common commands

Run from repo root unless noted.

```bash
# Backend
dotnet build                                                # all 7 projects (4 src + 3 test)
dotnet test                                                 # all 19 tests
dotnet test tests/EventManagement.Domain.Tests              # one test project
dotnet test --filter "FullyQualifiedName~RegistrationRulesTests"   # one class
dotnet test --filter "DisplayName~capacity"                 # tests matching a substring
dotnet run --project src/EventManagement.Api                # http://localhost:5050 + /swagger

# Frontend (cd src/EventManagement.Web first)
npm install
npm run dev          # http://localhost:5173, hot reload
npm run build        # tsc -b + vite build → dist/
npm run lint         # ESLint
```

The API must be running on `:5050` for the frontend to work — CORS only whitelists `http://localhost:5173`, and `VITE_API_BASE_URL` in `.env.development` points at `:5050`.

## Architecture: dependency direction is enforced by csproj references

```
EventManagement.Api  ──►  Application  ──►  Domain
        │                       ▲             ▲
        └───►  Infrastructure ──┘─────────────┘
```

Anything in `Domain` (entities, exceptions, `RegistrationRules`) has **zero** infrastructure dependencies — no HTTP, no logging, no clock-reading. Time enters via `IClock` (declared in `Application/Interfaces`, implemented by `SystemClock` in `Infrastructure`, faked by `FixedClock` in `Api.Tests/Helpers`). When adding domain logic, never call `DateTimeOffset.UtcNow` directly — accept the value as a parameter or inject `IClock`.

## Three patterns that recur

1. **Per-event lock in `RegistrationService`.** A `ConcurrentDictionary<Guid, SemaphoreSlim>` gates the check-count → check-rules → insert sequence so two simultaneous registrations to the same event can't both pass the capacity check. The lock is per-event, not global, so registrations to *different* events don't serialize. If you add another flow that mutates an event's collective state, follow the same shape.

2. **Exception → HTTP status mapping.** `ExceptionHandlingMiddleware` catches `EventNotFoundException` / `RegistrationNotFoundException` first (→ 404), then falls back to the `DomainException` base (→ 422), then `Exception` (→ 500). New domain rule violations should subclass `DomainException` — they automatically map to 422 without touching the middleware. DataAnnotations failures are returned as 400 with `ValidationProblemDetails` by `[ApiController]` and never reach the middleware.

3. **TanStack Query key hierarchy.** Three keys: `['events']`, `['events', id]`, `['events', id, 'registrations']`. Mutations invalidate the affected parents; for example, `useRegister(eventId)` invalidates all three because the registration count on the detail and the list both change. When adding a new mutation, keep this invariant — invalidate every key that includes the data your mutation touched.

## Test layout

| Project | Scope | Mocks |
|---|---|---|
| `EventManagement.Domain.Tests` | `RegistrationRules` pure-function tests | None (pass `_now` directly) |
| `EventManagement.Application.Tests` | `EventService`, `RegistrationService` | NSubstitute on repos + `IClock` |
| `EventManagement.Api.Tests` | End-to-end via `WebApplicationFactory<Program>` | Real in-memory repos + `FixedClock` |

`Api.Tests` instantiates a fresh `ApiFactory` (defined in `Helpers/`) per test class to keep in-memory state isolated. `Program.cs` ends with `public partial class Program { }` specifically so `WebApplicationFactory<Program>` can target it.

## Frontend layout

`src/EventManagement.Web/src/` is organized by feature, not by file type:

- `api/` — axios client and per-resource API modules
- `features/events/` and `features/registrations/` — each contains its own `hooks/`, `components/`, `pages/` (events only), and `types/`
- `shared/components/` — `Spinner`, `EmptyState`, `ErrorBanner`, `ConfirmModal` (no business logic)
- `shared/utils/` — `date.ts` (round-trips ISO 8601 ↔ `datetime-local` input value) and `apiError.ts` (parses `ValidationProblemDetails` vs. the custom `ErrorResponse` shape)

`EventForm` is shared between `CreateEventPage` and `EventDetailPage`'s inline-edit toggle. There is intentionally no `/events/:id/edit` route — edit happens in place on the detail page.

## Conventions

- Commit messages use Conventional Commits (`feat(domain):`, `feat(api):`, `chore:`, `docs:`, `test(api):`, `refactor:`). One commit per task in the plan.
- Every commit on this branch includes the `Co-Authored-By: Claude` trailer (created via the brainstorming → plan → subagent-driven-development workflow).
- Backend ports: API at `:5050` (configured in `launchSettings.json`). Vite at `:5173`.
- All dates are stored UTC server-side; the frontend converts to/from the user's local timezone at the boundary.
