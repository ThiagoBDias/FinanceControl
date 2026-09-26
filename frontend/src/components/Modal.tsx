import type { FormEvent, ReactNode } from "react";

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
  return <div className="modal-backdrop" onMouseDown={(event) => { if (event.target === event.currentTarget) onClose(); }}>
    <section className="modal-panel" role="dialog" aria-modal="true" aria-labelledby="modal-title">
      <div className="modal-head"><h2 id="modal-title">{title}</h2><button type="button" className="icon-btn" onClick={onClose} aria-label="Fechar">×</button></div>
      {onSubmit ? <form className="form" onSubmit={onSubmit}>{error && <div className="alert error" role="alert">{error}</div>}{children}<div className="modal-actions"><button type="button" className="secondary-btn" onClick={onClose} disabled={busy}>Cancelar</button><button type="submit" className="primary-btn" disabled={busy || submitDisabled}>{busy ? "Salvando…" : submitLabel}</button></div></form> : children}
    </section>
  </div>;
}
