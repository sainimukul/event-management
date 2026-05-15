# Event Management System — Design & Implementation Plan

This document is a complete execution plan for implementing the Event Management System take-home exercise. It is written so that any engineer or AI coding agent can use it as the single source of truth for building the solution.

The plan intentionally focuses on the actual exercise requirements first, while keeping the implementation clean, future-ready, and maintainable without unnecessary complexity.

---

## 1. Objective

Build a REST API and a small frontend for managing events and attendee registrations with the following capabilities:

- Create, read, and update events
- Register and unregister attendees for events
- Enforce the business rules:
  - Cannot register for past events
  - Cannot exceed event capacity
  - Cannot double-register the same user for the same event
- Use in-memory data storage
- Provide unit tests for business logic
- Provide a frontend using React (Vite) or allow use through Postman
- Keep authentication and authorization out of scope

The recommended implementation stack is:

- **Backend:** C# / ASP.NET Core (.NET 8)
- **Frontend:** React + Vite + TypeScript
- **Testing:** xUnit + FluentAssertions for backend, optional Vitest/RTL for frontend

---

## 2. Implementation principles

The implementation must follow these principles:

### 2.1 Keep the architecture right-sized

Use a simple layered modular design:
- API layer
- Application layer
- Domain layer
- Infrastructure layer

Do not introduce unnecessary enterprise patterns such as:
- distributed locking
- message queues
- outbox pattern
- microservices
- CQRS read models
- multi-tenant abstractions
- Redis abstractions
- event bus infrastructure

### 2.2 Keep controllers thin

Controllers should only:
- accept requests
- validate request models
- call application services
- return HTTP responses

Controllers must **not** implement business rules.

### 2.3 Keep business rules centralized

All business rules related to registration must be enforced in one place so they are easy to test and maintain.

### 2.4 Keep storage replaceable

Use repository interfaces so the in-memory implementation can later be replaced with a physical database implementation without changing domain or application logic.

### 2.5 Optimize for code quality and readability

This is a take-home exercise, so clarity matters more than theoretical completeness.

---

## 3. Functional scope

### 3.1 Events

An event supports:
- title
- description
- date
- max capacity

Required operations:
- create event
- get event by id
- list events
- update event

### 3.2 Registrations

A registration supports:
- user id
- user name
- event id
- registration timestamp

Required operations:
- register user to an event
- unregister user from an event
- list registrations for an event

### 3.3 Explicit out-of-scope features

Do not implement:
- authentication / authorization
- persistence to disk or DB
- notifications
- email sending
- recurring events
- waitlists
- soft delete
- audit history
- pagination unless there is enough time after core requirements are complete

---

## 4. Final architecture to implement

### 4.1 Overview

```text
EventManagement.Web (React + Vite)
        ↓ HTTP/JSON
ASP.NET Core API
        ↓
Application Services
        ↓
Domain Rules + Entities
        ↓
In-Memory Repositories
```

### 4.2 Why this architecture

This architecture is chosen because it:
- fully satisfies the exercise requirements
- makes business rules easy to unit test
- keeps the code easy to review
- supports later migration to a real database
- avoids over-engineering

---

## 5. Solution structure

Create the repository with the following top-level structure:

```text
EventManagement/
├── src/
│   ├── EventManagement.API/
│   ├── EventManagement.Application/
│   ├── EventManagement.Domain/
│   ├── EventManagement.Infrastructure/
│   └── EventManagement.Web/
├── tests/
│   ├── EventManagement.Domain.Tests/
│   ├── EventManagement.Application.Tests/
│   └── EventManagement.API.Tests/
└── README.md
```

### 5.1 Project structure under `src/`

```text
src/EventManagement.API/
├── Controllers/
│   ├── EventsController.cs
│   └── RegistrationsController.cs
├── Middleware/
│   ├── ExceptionHandlingMiddleware.cs
│   └── RequestLoggingMiddleware.cs
├── Models/
│   ├── Events/
│   └── Registrations/
├── Program.cs
└── appsettings.json
```

```text
src/EventManagement.Application/
├── Events/
│   ├── EventService.cs
│   ├── DTOs/
│   └── Requests/
├── Registrations/
│   ├── RegistrationService.cs
│   ├── DTOs/
│   └── Requests/
└── Interfaces/
    ├── IEventRepository.cs
    └── IRegistrationRepository.cs
```

```text
src/EventManagement.Domain/
├── Entities/
│   ├── Event.cs
│   └── Registration.cs
├── Exceptions/
│   ├── EventInPastException.cs
│   ├── EventCapacityExceededException.cs
│   └── DuplicateRegistrationException.cs
└── Services/
    └── RegistrationRules.cs
```

```text
src/EventManagement.Infrastructure/
├── Persistence/
│   ├── InMemoryEventRepository.cs
│   └── InMemoryRegistrationRepository.cs
└── DependencyInjection.cs
```

```text
src/EventManagement.Web/
├── src/
│   ├── api/
│   │   ├── eventsApi.ts
│   │   └── registrationsApi.ts
│   ├── features/
│   │   ├── events/
│   │   │   ├── components/
│   │   │   ├── hooks/
│   │   │   └── pages/
│   │   └── registrations/
│   │       ├── components/
│   │       ├── hooks/
│   │       └── types/
│   ├── shared/
│   │   ├── components/
│   │   └── utils/
│   ├── App.tsx
│   └── main.tsx
├── index.html
├── package.json
├── tsconfig.json
└── vite.config.ts
```

---

## 6. Backend domain model

### 6.1 Event entity

Implement an `Event` entity with the following properties:

| Property | Type | Notes |
|---|---|---|
| `Id` | `Guid` | Unique identifier |
| `Title` | `string` | Required |
| `Description` | `string?` | Optional |
| `Date` | `DateTimeOffset` | Store in UTC where possible |
| `MaxCapacity` | `int` | Must be > 0 |
| `CreatedAt` | `DateTimeOffset` | Set on creation |
| `UpdatedAt` | `DateTimeOffset` | Updated on modification |

### 6.2 Registration entity

Implement a `Registration` entity with the following properties:

| Property | Type | Notes |
|---|---|---|
| `Id` | `Guid` | Unique identifier |
| `EventId` | `Guid` | Foreign-key-like reference |
| `UserId` | `string` | Required |
| `UserName` | `string` | Required |
| `RegisteredAt` | `DateTimeOffset` | Set on creation |

### 6.3 Domain exceptions

Create explicit exceptions for business rule failures:

- `EventInPastException`
- `EventCapacityExceededException`
- `DuplicateRegistrationException`

Optional additional exceptions:
- `EventNotFoundException`
- `RegistrationNotFoundException`

---

## 7. Business rules design

Create a domain service named `RegistrationRules`.

### Responsibilities

It must expose methods that validate:
- event is not in the past
- event has capacity remaining
- user is not already registered for the event

### Important rule

This class should contain only business rule logic.
It should not know anything about HTTP, controllers, logging, or persistence.

### Suggested public methods

```text
EnsureEventIsNotInPast(Event event)
EnsureCapacityAvailable(Event event, int currentRegistrationCount)
EnsureUserNotAlreadyRegistered(string userId, IEnumerable<Registration> registrations)
```

### Why this matters

Keeping rules here allows direct unit testing with no infrastructure setup.

---

## 8. Repository design

### 8.1 Interfaces

Define two repository interfaces in the Application layer.

#### IEventRepository

Required methods:
- `GetAllAsync()`
- `GetByIdAsync(Guid id)`
- `AddAsync(Event event)`
- `UpdateAsync(Event event)`

#### IRegistrationRepository

Required methods:
- `GetByEventIdAsync(Guid eventId)`
- `GetByIdAsync(Guid id)`
- `AddAsync(Registration registration)`
- `DeleteAsync(Guid registrationId)`
- `ExistsAsync(Guid eventId, string userId)`
- `CountByEventIdAsync(Guid eventId)`

### 8.2 In-memory implementation

Use thread-safe collections:
- `ConcurrentDictionary<Guid, Event>`
- `ConcurrentDictionary<Guid, Registration>`

### 8.3 Concurrency handling

Because the application is local and in-memory only, use a process-level lock around the registration flow.

Recommended approach:
- Add a private `SemaphoreSlim` or `lock` object in `RegistrationService`
- Wrap the critical path of registration inside it

This ensures that the sequence:
- check current count
- check duplicate
- insert registration

is safe inside a single process.

This is sufficient for the exercise.

---

## 9. Application services design

### 9.1 EventService

Create `EventService` in the Application layer.

### Responsibilities

- Create event
- Get event by id
- List events
- Update event

### Validation responsibilities

It should validate or enforce:
- title is required
- date is valid
- max capacity is positive
- event exists before update

### 9.2 RegistrationService

Create `RegistrationService` in the Application layer.

### Responsibilities

- Register attendee
- Unregister attendee
- List registrations for event

### Registration workflow

When registering a user:
1. Load event by `eventId`
2. If event not found, throw not-found exception
3. Acquire lock/semaphore
4. Load registrations for event
5. Apply `RegistrationRules`
6. Create registration
7. Save registration
8. Release lock
9. Return created registration DTO

### Unregistration workflow

When unregistering:
1. Load registration by id
2. If not found, return not-found
3. Delete registration
4. Return success

---

## 10. Request and response models

### 10.1 Event requests

Create these request models:

#### CreateEventRequest
- `title`
- `description`
- `date`
- `maxCapacity`

#### UpdateEventRequest
- `title`
- `description`
- `date`
- `maxCapacity`

### 10.2 Registration request

#### RegisterUserRequest
- `userId`
- `userName`

### 10.3 DTOs

Create response DTOs:
- `EventDto`
- `EventDetailDto`
- `RegistrationDto`

Do not expose internal entity types directly from controllers.

---

## 11. API design

### 11.1 Events endpoints

| Method | Route | Purpose |
|---|---|---|
| `GET` | `/api/events` | List all events |
| `GET` | `/api/events/{id}` | Get single event |
| `POST` | `/api/events` | Create event |
| `PUT` | `/api/events/{id}` | Update event |

### 11.2 Registrations endpoints

| Method | Route | Purpose |
|---|---|---|
| `GET` | `/api/events/{eventId}/registrations` | List registrations for event |
| `POST` | `/api/events/{eventId}/registrations` | Register user for event |
| `DELETE` | `/api/events/{eventId}/registrations/{registrationId}` | Unregister user |

### 11.3 Response status mapping

| Scenario | Status code |
|---|---|
| List/get/update success | `200 OK` |
| Create/register success | `201 Created` |
| Delete success | `204 No Content` |
| Invalid request | `400 Bad Request` |
| Resource not found | `404 Not Found` |
| Business rule violation | `422 Unprocessable Entity` |
| Unexpected error | `500 Internal Server Error` |

---

## 12. Validation design

Validation can be implemented either with FluentValidation or standard ASP.NET model validation.

### Required checks

#### Event input
- title must not be empty
- title max length should be reasonable (for example 200)
- description max length should be reasonable
- date must be provided
- max capacity must be greater than zero

#### Registration input
- userId must not be empty
- userName must not be empty

### Recommendation

Use FluentValidation if time permits because it keeps request validation cleaner and testable.
Otherwise, use DataAnnotations for simplicity.

---

## 13. Exception handling and error responses

Implement a global exception middleware.

### Responsibilities

- Catch exceptions from controllers/services
- Map known exceptions to HTTP status codes
- Return a consistent JSON error payload
- Log unexpected errors

### Suggested mapping

| Exception | Response |
|---|---|
| `EventNotFoundException` | `404` |
| `RegistrationNotFoundException` | `404` |
| `EventInPastException` | `422` |
| `EventCapacityExceededException` | `422` |
| `DuplicateRegistrationException` | `422` |
| Validation exception | `400` |
| Unknown exception | `500` |

### Error response shape

```json
{
  "message": "Event capacity exceeded",
  "statusCode": 422,
  "details": null
}
```

Keep the error contract simple and consistent.

---

## 14. Logging plan

Use structured logging with Serilog.

### Minimum logging requirements

Log:
- incoming request method and path
- outgoing response status code
- request duration
- business rule failures
- unexpected exceptions

### Suggested fields

- `Method`
- `Path`
- `StatusCode`
- `ElapsedMs`
- `EventId` when relevant
- `UserId` when relevant

### Log levels

| Level | Usage |
|---|---|
| `Information` | request lifecycle logs |
| `Warning` | business rule violations |
| `Error` | unexpected exceptions |

Do not over-log every internal method call.
Keep logs useful and readable.

---

## 15. Web application implementation plan

### 15.1 Stack

- React
- Vite
- TypeScript
- Axios or Fetch API
- Basic CSS or a lightweight component library if needed

The frontend project name should be **`EventManagement.Web`** and it should live under `src/` like the other projects.

### 15.2 Frontend pages

Create the following screens:

#### Events List Page
Shows all events with:
- title
- date
- max capacity
- button to view details
- button to edit

#### Create Event Page
Form with:
- title
- description
- date
- max capacity

#### Event Detail Page
Shows:
- event details
- current registrations
- registration form
- unregister actions

#### Edit Event Page
Allows updating event fields.

### 15.3 Frontend feature folders

```text
src/EventManagement.Web/src/
├── api/
│   ├── eventsApi.ts
│   └── registrationsApi.ts
├── features/
│   ├── events/
│   │   ├── components/
│   │   ├── pages/
│   │   └── hooks/
│   └── registrations/
│       ├── components/
│       ├── hooks/
│       └── types/
├── shared/
│   ├── components/
│   └── utils/
└── main.tsx
```

### 15.4 Frontend priorities

If time is limited, prioritize:
1. Event list
2. Event create
3. Event detail with registration form
4. Event edit

Keep the UI simple, clean, and functional.

---

## 16. Testing plan

### 16.1 Must-have tests

These are mandatory.

#### Domain tests
Create tests for `RegistrationRules`:
- registration succeeds for future event with available capacity
- registration fails for past event
- registration fails when capacity is full
- registration fails when same user is already registered

#### Application tests
Create tests for `RegistrationService`:
- register user success path
- unregister success path
- event not found case
- duplicate case
- capacity exceeded case

Create tests for `EventService`:
- create event success
- update event success
- get event by id

### 16.2 Optional API integration tests

If time permits, add:
- `POST /api/events` returns `201`
- `POST /api/events/{id}/registrations` returns `201`
- invalid registration returns `422`

### 16.3 Test priority order

Build tests in this order:
1. `RegistrationRules`
2. `RegistrationService`
3. `EventService`
4. API integration tests

---

## 17. Step-by-step execution plan for an AI agent

This section is the implementation sequence to follow exactly.

### Phase 1 — Backend foundation

1. Create the .NET solution and four projects:
   - API
   - Application
   - Domain
   - Infrastructure
2. Add project references:
   - API → Application + Infrastructure
   - Application → Domain
   - Infrastructure → Application + Domain
3. Configure dependency injection in API startup
4. Add Swagger/OpenAPI
5. Add Serilog

### Phase 2 — Domain layer

1. Implement `Event` entity
2. Implement `Registration` entity
3. Implement domain exceptions
4. Implement `RegistrationRules`
5. Write unit tests for `RegistrationRules`

### Phase 3 — Application layer

1. Define repository interfaces
2. Create request models and DTOs
3. Implement `EventService`
4. Implement `RegistrationService`
5. Write unit tests for services using mocked repositories

### Phase 4 — Infrastructure layer

1. Implement `InMemoryEventRepository`
2. Implement `InMemoryRegistrationRepository`
3. Register repositories in DI
4. Add thread-safety handling in registration path
5. Add repository tests if needed

### Phase 5 — API layer

1. Implement `EventsController`
2. Implement `RegistrationsController`
3. Add validation
4. Add exception middleware
5. Add request logging middleware
6. Verify Swagger works
7. Add integration tests if time allows

### Phase 6 — Frontend

1. Create `src/EventManagement.Web` as a Vite React TypeScript app
2. Create API client files
3. Build Event List page
4. Build Create Event page
5. Build Event Detail page with registration form
6. Build Edit Event page
7. Add basic error and loading states

### Phase 7 — Finalization

1. Run all tests
2. Clean code and remove dead code
3. Verify all requirements manually using UI or Swagger/Postman
4. Write README
5. Capture assumptions and future improvement notes

---

## 18. Manual verification checklist

Before submission, verify all of the following manually.

### Events
- Can create an event
- Can list events
- Can get event detail
- Can update event

### Registrations
- Can register a user to a future event
- Cannot register same user twice
- Cannot register beyond max capacity
- Cannot register for past event
- Can unregister a user

### API quality
- Correct status codes returned
- Error responses are consistent
- Swagger opens correctly
- Invalid payload returns validation errors

### Frontend quality
- Event list works
- Event creation works
- Registration works
- Errors are shown clearly
- UI is simple and understandable

### Tests
- Business rules covered
- Service behavior covered
- All tests pass locally

---

## 19. README content plan

The README should include:

### 19.1 Project overview
Short explanation of what the app does.

### 19.2 Tech stack
- ASP.NET Core
- React + Vite + TypeScript
- In-memory storage
- xUnit

### 19.3 How to run backend
Commands:
```bash
cd src/EventManagement.API
# restore/build/run instructions
```

### 19.4 How to run frontend
Commands:
```bash
cd src/EventManagement.Web
# npm install / npm run dev
```

### 19.5 How to run tests
Commands:
```bash
dotnet test
cd src/EventManagement.Web && npm test
```

### 19.6 API notes
- Base URL
- Swagger URL
- Important endpoints

### 19.7 Assumptions
Examples:
- `userId` is provided in request body because auth is out of scope
- in-memory state resets on restart
- delete event endpoint is intentionally omitted if not required by exercise wording

### 19.8 Future improvements
Mention only a small set:
- physical database
- pagination
- auth
- Docker
- caching

---

## 20. Quality bar for submission

The final submission should demonstrate:

- clear separation of concerns
- correct implementation of business rules
- readable project structure
- good naming
- good error handling
- useful logs
- meaningful tests
- simple but functional frontend
- clear README

The code should feel like a professional, maintainable v1 — not a toy demo and not an over-engineered enterprise template.

---

## 21. Common mistakes to avoid

Do not do the following:

- Put business rules directly in controllers
- Return entities directly from controllers
- Use non-thread-safe collections for in-memory storage
- Skip tests for the three required business rules
- Overbuild with patterns not needed for the exercise
- Spend too much time polishing UI while backend rules remain under-tested
- Add authentication even though it is out of scope
- Hardcode random inconsistent error responses

---

## 22. Final delivery expectation

The final implementation should include:

- working backend API
- working web UI in `EventManagement.Web` or clear Postman usage path
- unit tests for business rules
- clean project structure
- README with setup instructions

If time becomes constrained, the priority order is:

1. Correct backend business logic
2. Unit tests
3. Clean API endpoints
4. Basic frontend
5. Extra polish

This order maximizes evaluation score against the exercise criteria.

---

## 23. Agent execution instruction summary

Any AI coding agent following this document must prioritize in this exact order:

1. Create project structure
2. Implement domain entities and business rule tests
3. Implement application services
4. Implement in-memory repositories
5. Implement API controllers and middleware
6. Verify requirements through Swagger/tests
7. Implement `EventManagement.Web`
8. Write README
9. Final cleanup and verification

The agent should avoid adding any feature not required unless it clearly improves maintainability with very low complexity cost.

---

## 24. Final note

This plan is intentionally optimized for the take-home exercise. It is future-aware, but not future-burdened. The main objective is to produce a clean, correct, testable submission that demonstrates strong engineering judgment.
