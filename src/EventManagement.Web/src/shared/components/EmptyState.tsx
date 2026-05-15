import type { ReactNode } from "react";
import styles from "./EmptyState.module.css";

export function EmptyState({ title, action }: { title: string; action?: ReactNode }) {
  return (
    <div className={styles.wrapper}>
      <p className="muted">{title}</p>
      {action}
    </div>
  );
}
