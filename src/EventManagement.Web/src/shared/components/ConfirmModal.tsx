import styles from "./ConfirmModal.module.css";

interface Props {
  open: boolean;
  title: string;
  message: string;
  confirmLabel?: string;
  onConfirm: () => void;
  onCancel: () => void;
}

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
