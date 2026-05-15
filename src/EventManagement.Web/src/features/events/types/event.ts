/**
 * Lightweight event projection returned by the list and create endpoints. Mirrors the
 * backend's `EventDto`. Dates are ISO 8601 strings (UTC) and converted to the user's
 * timezone at render time by `formatDate`.
 */
export interface EventSummary {
  /** Stable event identifier (GUID string). */
  id: string;
  /** Display title. */
  title: string;
  /** Event date/time as ISO 8601, UTC. */
  date: string;
  /** Maximum number of registrations the event accepts. */
  maxCapacity: number;
  /** Current registration count at the time of the response. */
  currentRegistrations: number;
  /** When the event was created, ISO 8601 UTC. */
  createdAt: string;
}

/**
 * Full event projection returned by the detail and update endpoints. Mirrors the backend's
 * `EventDetailDto` and extends {@link EventSummary} with description and update timestamp.
 */
export interface EventDetail extends EventSummary {
  /** Optional long-form description. `null` when not set. */
  description?: string | null;
  /** When the event was last modified, ISO 8601 UTC. */
  updatedAt: string;
}

/**
 * Local form state used by `EventForm`. The `date` field carries the value from
 * `<input type="datetime-local">` (local time, no zone) and is converted to a UTC ISO
 * string at the submit boundary via `fromLocalInputValue`.
 */
export interface EventFormValues {
  title: string;
  description: string;
  /** `<input type="datetime-local">` value, e.g. `"2026-06-01T18:30"`. */
  date: string;
  maxCapacity: number;
}
