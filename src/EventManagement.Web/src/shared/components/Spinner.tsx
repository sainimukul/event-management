import styles from "./Spinner.module.css";

/**
 * Small loading indicator with an accessible label announced to screen readers via
 * `role="status"` + `aria-live="polite"`. The label defaults to "Loading..." but can be
 * customised per use site.
 */
export function Spinner({ label = "Loading..." }: { label?: string }) {
  return (
    <div className={styles.wrapper} role="status" aria-live="polite">
      <div className={styles.dot} />
      <span className="muted">{label}</span>
    </div>
  );
}
