import styles from "./Spinner.module.css";

export function Spinner({ label = "Loading..." }: { label?: string }) {
  return (
    <div className={styles.wrapper} role="status" aria-live="polite">
      <div className={styles.dot} />
      <span className="muted">{label}</span>
    </div>
  );
}
