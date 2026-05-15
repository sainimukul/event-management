import { Link } from "react-router-dom";
import type { EventSummary } from "../types/event";
import { formatDate } from "../../../shared/utils/date";
import styles from "./EventCard.module.css";

/**
 * Compact event tile used on the list page. Wraps the whole card in a `<Link>` so the
 * entire surface is clickable, and derives the "Full" / "Past" badges client-side from
 * the event's date and counts — the backend doesn't return a status field, so this is
 * the single source of truth for tile state.
 */
export function EventCard({ event }: { event: EventSummary }) {
  const full = event.currentRegistrations >= event.maxCapacity;
  const past = new Date(event.date).getTime() < Date.now();
  return (
    <Link to={`/events/${event.id}`} className={styles.card}>
      <div className={styles.title}>{event.title}</div>
      <div className="muted">{formatDate(event.date)}</div>
      <div className={styles.meta}>
        <span>{event.currentRegistrations} / {event.maxCapacity} registered</span>
        {full && !past && <span className={styles.badgeFull}>Full</span>}
        {past && <span className={styles.badgePast}>Past</span>}
      </div>
    </Link>
  );
}
