import type { ReactNode } from "react";
import styles from "./EmptyState.module.css";

/**
 * Placeholder shown when a collection comes back empty (no events, no registrations).
 * The optional `action` slot lets the page suggest a next step, e.g. a "Create event"
 * button on the list view.
 */
export function EmptyState({ title, action }: { title: string; action?: ReactNode }) {
  return (
    <div className={styles.wrapper}>
      <p className="muted">{title}</p>
      {action}
    </div>
  );
}
