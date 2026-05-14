import { useState } from "react";
import { useParams, Link, useNavigate } from "react-router-dom";
import toast from "react-hot-toast";
import { useEvent } from "../hooks/useEvent";
import { useUpdateEvent } from "../hooks/useUpdateEvent";
import { EventForm } from "../components/EventForm";
import { RegisterForm } from "../../registrations/components/RegisterForm";
import { RegistrationList } from "../../registrations/components/RegistrationList";
import { Spinner } from "../../../shared/components/Spinner";
import { ErrorBanner } from "../../../shared/components/ErrorBanner";
import { formatDate } from "../../../shared/utils/date";
import styles from "./EventDetailPage.module.css";

export function EventDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: ev, isLoading, isError, refetch } = useEvent(id);
  const updateMutation = useUpdateEvent(id!);
  const [editing, setEditing] = useState(false);

  if (isLoading) return <Spinner label="Loading event..." />;
  if (isError || !ev) return <ErrorBanner message="Failed to load event." onRetry={() => refetch()} />;

  const past = new Date(ev.date).getTime() < Date.now();
  const full = ev.currentRegistrations >= ev.maxCapacity;
  const canRegister = !past && !full;

  return (
    <section>
      <div className="page-header">
        <Link to="/" className="button button--ghost">← Back</Link>
        {!editing && <button type="button" className="button" onClick={() => setEditing(true)}>Edit</button>}
      </div>

      {!editing ? (
        <div className={`card ${styles.summary}`}>
          <h1 className={styles.title}>{ev.title}</h1>
          <div className="muted">{formatDate(ev.date)}</div>
          {ev.description && <p>{ev.description}</p>}
          <div className={styles.meta}>
            <span>{ev.currentRegistrations} / {ev.maxCapacity} registered</span>
            {past && <span className={styles.badgePast}>Past</span>}
            {full && !past && <span className={styles.badgeFull}>Full</span>}
          </div>
        </div>
      ) : (
        <EventForm
          mode="edit"
          initialData={ev}
          onSubmit={async (payload) => {
            await updateMutation.mutateAsync(payload);
            toast.success("Event updated");
            setEditing(false);
          }}
          onCancel={() => setEditing(false)}
        />
      )}

      <h2 className={styles.sectionHeading}>Registrations</h2>
      <RegistrationList eventId={ev.id} />

      {canRegister && <RegisterForm eventId={ev.id} />}
      {!canRegister && (
        <div className="card muted">
          {past ? "This event is in the past." : "This event is full."}
        </div>
      )}

      <p className={styles.footerLink}>
        <button type="button" className="button button--ghost" onClick={() => navigate("/")}>Back to all events</button>
      </p>
    </section>
  );
}
