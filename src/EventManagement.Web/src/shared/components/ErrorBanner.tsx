import styles from "./ErrorBanner.module.css";

/**
 * Inline error banner with an optional retry callback. Uses `role="alert"` so the message
 * is announced to assistive tech as soon as it appears. Pair this with `extractApiError`
 * to produce a uniform message string across backend error shapes.
 */
export function ErrorBanner({ message, onRetry }: { message: string; onRetry?: () => void }) {
  return (
    <div className={styles.wrapper} role="alert">
      <span>{message}</span>
      {onRetry && (
        <button type="button" className="button button--ghost" onClick={onRetry}>
          Retry
        </button>
      )}
    </div>
  );
}
