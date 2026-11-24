import { useEffect, useId, useRef, type FormEvent, type ReactNode } from "react";

type Props = {
  title: string;
  onClose: () => void;
  children: ReactNode;
  onSubmit?: (event: FormEvent<HTMLFormElement>) => void;
  submitLabel?: string;
  busy?: boolean;
  submitDisabled?: boolean;
  error?: string;
};

export default function Modal({ title, onClose, children, onSubmit, submitLabel = "Salvar", busy = false, submitDisabled = false, error = "" }: Props) {
  const dialog = useRef<HTMLDialogElement>(null);
  const titleId = useId();
  useEffect(() => {
    const element = dialog.current;
    element?.showModal();
    return () => { element?.close(); };
  }, []);

  return <dialog ref={dialog} className="modal-panel" aria-labelledby={titleId} onCancel={(event) => {
    event.preventDefault();
    if (!busy) onClose();
  }}>
    <div className="modal-head"><h2 id={titleId}>{title}</h2>
      <button type="button" className="icon-btn" onClick={onClose} disabled={busy} aria-label="Fechar">×</button>
    </div>
    {onSubmit ? <form className="form" onSubmit={onSubmit}>
      {error && <div className="alert error" role="alert">{error}</div>}
      {children}
      <div className="modal-actions">
        <button type="button" className="secondary-btn" onClick={onClose} disabled={busy}>Cancelar</button>
        <button type="submit" className="primary-btn" disabled={busy || submitDisabled}>{busy ? "Salvando…" : submitLabel}</button>
      </div>
    </form> : children}
  </dialog>;
}
