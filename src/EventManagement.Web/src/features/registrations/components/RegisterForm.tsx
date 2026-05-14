import { useState, type FormEvent } from "react";
import toast from "react-hot-toast";
import { useRegister } from "../hooks/useRegister";
import { extractApiError } from "../../../shared/utils/apiError";
import styles from "./RegisterForm.module.css";

export function RegisterForm({ eventId, disabled }: { eventId: string; disabled?: boolean }) {
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
          value={userId} onChange={(e) => setUserId(e.target.value)} disabled={disabled} />
      </div>
      <div className="form-row">
        <label className="label" htmlFor="userName">User name</label>
        <input id="userName" className="input" required maxLength={200}
          value={userName} onChange={(e) => setUserName(e.target.value)} disabled={disabled} />
      </div>
      {error && <div className="field-error" role="alert">{error}</div>}
      <button type="submit" className="button button--primary" disabled={disabled || mutation.isPending}>
        {mutation.isPending ? "Registering..." : "Register"}
      </button>
    </form>
  );
}
