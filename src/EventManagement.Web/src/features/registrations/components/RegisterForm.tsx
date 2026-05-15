import { useState, type FormEvent } from "react";
import toast from "react-hot-toast";
import { useRegister } from "../hooks/useRegister";
import { extractApiError } from "../../../shared/utils/apiError";
import styles from "./RegisterForm.module.css";

/**
 * Inline form for registering a new attendee to an event. Rendered on the event detail page
 * only when the event isn't past or full — the parent makes that decision and either shows
 * this component or a "this event is closed" notice. On success toasts and resets the form
 * so the same operator can register several attendees in a row.
 */
export function RegisterForm({ eventId }: { eventId: string }) {
  const [userId, setUserId] = useState("");
  const [userName, setUserName] = useState("");
  const [error, setError] = useState<string | null>(null);
  const mutation = useRegister(eventId);

  async function onSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    try {
      await mutation.mutateAsync({ userId: userId.trim(), userName: userName.trim() });
      toast.success("Registered");
      setUserId("");
      setUserName("");
    } catch (err) {
      setError(extractApiError(err).message);
    }
  }

  return (
    <form className={styles.form} onSubmit={onSubmit}>
      <h3 className={styles.heading}>Register a new attendee</h3>
      <div className="form-row">
        <label className="label" htmlFor="userId">User ID</label>
        <input id="userId" className="input" required maxLength={100}
          value={userId} onChange={(e) => setUserId(e.target.value)} />
      </div>
      <div className="form-row">
        <label className="label" htmlFor="userName">User name</label>
        <input id="userName" className="input" required maxLength={200}
          value={userName} onChange={(e) => setUserName(e.target.value)} />
      </div>
      {error && <div className="field-error" role="alert">{error}</div>}
      <button type="submit" className="button button--primary" disabled={mutation.isPending}>
        {mutation.isPending ? "Registering..." : "Register"}
      </button>
    </form>
  );
}
