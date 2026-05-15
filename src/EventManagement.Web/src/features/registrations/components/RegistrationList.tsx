import { useState } from "react";
import toast from "react-hot-toast";
import { useRegistrations } from "../hooks/useRegistrations";
import { useUnregister } from "../hooks/useUnregister";
import { Spinner } from "../../../shared/components/Spinner";
import { EmptyState } from "../../../shared/components/EmptyState";
import { ErrorBanner } from "../../../shared/components/ErrorBanner";
import { ConfirmModal } from "../../../shared/components/ConfirmModal";
import { formatDate } from "../../../shared/utils/date";
import { extractApiError } from "../../../shared/utils/apiError";
import styles from "./RegistrationList.module.css";

export function RegistrationList({ eventId }: { eventId: string }) {
  const { data, isLoading, isError, refetch } = useRegistrations(eventId);
  const unregister = useUnregister(eventId);
  const [pendingId, setPendingId] = useState<string | null>(null);

  if (isLoading) return <Spinner label="Loading registrations..." />;
  if (isError) return <ErrorBanner message="Failed to load registrations." onRetry={() => refetch()} />;
  if (!data || data.length === 0) return <EmptyState title="No one has registered yet." />;

  return (
    <>
      <ul className={styles.list}>
        {data.map((r) => (
          <li key={r.id} className={styles.row}>
            <div>
              <div className={styles.name}>{r.userName}</div>
              <div className="muted">{r.userId} · {formatDate(r.registeredAt)}</div>
            </div>
            <button type="button" className="button button--ghost" onClick={() => setPendingId(r.id)}>
              Unregister
            </button>
          </li>
        ))}
      </ul>
      <ConfirmModal
        open={pendingId !== null}
        title="Unregister attendee?"
        message="The attendee will be removed from this event."
        confirmLabel="Unregister"
        onCancel={() => setPendingId(null)}
        onConfirm={async () => {
          const id = pendingId!;
          setPendingId(null);
          try {
            await unregister.mutateAsync(id);
            toast.success("Unregistered");
          } catch (err) {
            toast.error(extractApiError(err).message);
          }
        }}
      />
    </>
  );
}
