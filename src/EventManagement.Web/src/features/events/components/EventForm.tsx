import { useState, type FormEvent } from "react";
import type { EventDetail, EventFormValues } from "../types/event";
import { toLocalInputValue, fromLocalInputValue } from "../../../shared/utils/date";
import { extractApiError } from "../../../shared/utils/apiError";
import styles from "./EventForm.module.css";

interface Props {
  mode: "create" | "edit";
  initialData?: EventDetail;
  onSubmit: (payload: { title: string; description: string | null; date: string; maxCapacity: number }) => Promise<unknown>;
  onCancel?: () => void;
  submitLabel?: string;
}

export function EventForm({ mode, initialData, onSubmit, onCancel, submitLabel }: Props) {
  const [values, setValues] = useState<EventFormValues>({
    title: initialData?.title ?? "",
    description: initialData?.description ?? "",
    date: initialData ? toLocalInputValue(initialData.date) : "",
    maxCapacity: initialData?.maxCapacity ?? 10,
  });
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<Record<string, string[]>>({});

  function set<K extends keyof EventFormValues>(key: K, value: EventFormValues[K]) {
    setValues((v) => ({ ...v, [key]: value }));
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setSubmitting(true);
    setError(null);
    setFieldErrors({});
    try {
      await onSubmit({
        title: values.title.trim(),
        description: values.description.trim() === "" ? null : values.description.trim(),
        date: fromLocalInputValue(values.date),
        maxCapacity: Number(values.maxCapacity),
      });
    } catch (err) {
      const info = extractApiError(err);
      setError(info.message);
      if (info.fieldErrors) setFieldErrors(info.fieldErrors);
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <form className={styles.form} onSubmit={handleSubmit} noValidate>
      <div className="form-row">
        <label className="label" htmlFor="title">Title</label>
        <input id="title" className="input" required maxLength={200}
          value={values.title} onChange={(e) => set("title", e.target.value)} />
        {fieldErrors.Title?.map((m) => <span key={m} className="field-error">{m}</span>)}
      </div>
      <div className="form-row">
        <label className="label" htmlFor="description">Description</label>
        <textarea id="description" className="textarea" maxLength={2000}
          value={values.description} onChange={(e) => set("description", e.target.value)} />
        {fieldErrors.Description?.map((m) => <span key={m} className="field-error">{m}</span>)}
      </div>
      <div className="form-row">
        <label className="label" htmlFor="date">Date and time</label>
        <input id="date" type="datetime-local" className="input" required
          value={values.date} onChange={(e) => set("date", e.target.value)} />
        {fieldErrors.Date?.map((m) => <span key={m} className="field-error">{m}</span>)}
      </div>
      <div className="form-row">
        <label className="label" htmlFor="maxCapacity">Max capacity</label>
        <input id="maxCapacity" type="number" min={1} className="input" required
          value={values.maxCapacity} onChange={(e) => set("maxCapacity", Number(e.target.value))} />
        {fieldErrors.MaxCapacity?.map((m) => <span key={m} className="field-error">{m}</span>)}
      </div>

      {error && <div className="field-error" role="alert">{error}</div>}

      <div className={styles.actions}>
        {onCancel && (
          <button type="button" className="button" onClick={onCancel} disabled={submitting}>
            Cancel
          </button>
        )}
        <button type="submit" className="button button--primary" disabled={submitting}>
          {submitting ? "Saving..." : submitLabel ?? (mode === "create" ? "Create event" : "Save changes")}
        </button>
      </div>
    </form>
  );
}
