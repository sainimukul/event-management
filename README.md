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
