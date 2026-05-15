// Convert an ISO 8601 string (UTC) to the value expected by <input type="datetime-local">.
export function toLocalInputValue(iso: string): string {
  const d = new Date(iso);
  const pad = (n: number) => String(n).padStart(2, "0");
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
}

// Convert the value from <input type="datetime-local"> (local time, no tz) to a UTC ISO 8601 string.
export function fromLocalInputValue(localValue: string): string {
  return new Date(localValue).toISOString();
}

export function formatDate(iso: string): string {
  return new Intl.DateTimeFormat(undefined, {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(iso));
}
