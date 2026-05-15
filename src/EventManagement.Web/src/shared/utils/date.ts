/**
 * Date utilities for round-tripping between the API's UTC ISO 8601 timestamps and the
 * `<input type="datetime-local">` value format. Kept in `shared/` because both event
 * creation and inline edit on the detail page need them.
 */

/**
 * Convert an ISO 8601 timestamp (UTC) into the value shape required by
 * `<input type="datetime-local">` (`YYYY-MM-DDTHH:mm`, local time, no timezone). The
 * `Date` constructor parses the ISO string in UTC, and the `get*` methods then return the
 * local-zone components — so the resulting string is the same wall-clock moment the user
 * sees, expressed in their browser's timezone.
 */
export function toLocalInputValue(iso: string): string {
  const d = new Date(iso);
  const pad = (n: number) => String(n).padStart(2, "0");
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
}

/**
 * Convert a `<input type="datetime-local">` value (local time, no zone) back into a UTC
 * ISO 8601 string suitable for the API. `new Date(localValue)` treats the string as local
 * time, and `.toISOString()` emits the UTC equivalent.
 */
export function fromLocalInputValue(localValue: string): string {
  return new Date(localValue).toISOString();
}

/**
 * Format an ISO 8601 timestamp for display, using the browser's locale and timezone.
 * Renders both date and time at medium/short widths — adjust the options if a screen
 * needs a different style.
 */
export function formatDate(iso: string): string {
  return new Intl.DateTimeFormat(undefined, {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(iso));
}
