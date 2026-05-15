import styles from "./ConfirmModal.module.css";

interface Props {
  /** When false the modal renders nothing. */
  open: boolean;
  /** Title shown at the top of the dialog. */
  title: string;
  /** Body message — usually a short sentence describing what's about to happen. */
  message: string;
  /** Label on the confirm button. Defaults to "Confirm". Use "Delete", "Unregister", etc. for destructive actions. */
  confirmLabel?: string;
  /** Called when the user clicks the confirm button. */
  onConfirm: () => void;
  /** Called when the user clicks Cancel or otherwise dismisses the dialog. */
  onCancel: () => void;
}

/**
 * Generic confirm-or-cancel modal. Renders as a fixed-position backdrop + centred dialog,
 * gated on `open` so the caller controls visibility. The destructive variant of the
 * confirm button (`button--danger`) is hard-coded — refactor to a `variant` prop if a
 * non-destructive confirm shows up later.
 */
export function ConfirmModal({ open, title, message, confirmLabel = "Confirm", onConfirm, onCancel }: Props) {
  if (!open) return null;
  return (
    <div className={styles.backdrop} role="dialog" aria-modal="true">
      <div className={styles.modal}>
        <h2 className={styles.title}>{title}</h2>
        <p className="muted">{message}</p>
        <div className={styles.actions}>
          <button type="button" className="button" onClick={onCancel}>Cancel</button>
          <button type="button" className="button button--danger" onClick={onConfirm}>{confirmLabel}</button>
        </div>
      </div>
    </div>
  );
}
