import { useCallback, useEffect, useState, type FormEvent } from "react";
import api, { getErrorMessage } from "../../services/api";
import { notifyFinanceDataChanged, onFinanceDataChanged } from "../../services/financeEvents";
import type { Account } from "../../types";
import PageHeader from "../../components/PageHeader";
import EmptyState from "../../components/EmptyState";
import Loading from "../../components/Loading";
import Modal from "../../components/Modal";

const money = (value: number) => new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" }).format(value);

export default function Cards() {
  const [items, setItems] = useState<Account[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [notice, setNotice] = useState("");
  const [createOpen, setCreateOpen] = useState(false);
  const [busy, setBusy] = useState(false);

  const load = useCallback(async () => {
    setError("");
    try { const { data } = await api.get<Account[]>("/credit-cards"); setItems(Array.isArray(data) ? data : []); }
    catch (err) { setError(getErrorMessage(err)); }
    finally { setLoading(false); }
  }, []);
  useEffect(() => { void load(); return onFinanceDataChanged(() => { void load(); }); }, [load]);

  async function createCard(event: FormEvent<HTMLFormElement>) {
    event.preventDefault(); setBusy(true); setError("");
    const form = new FormData(event.currentTarget);
    try {
      await api.post("/credit-cards", {
        name: String(form.get("name")).trim(), creditLimit: String(form.get("creditLimit")),
        closingDay: Number(form.get("closingDay")), dueDay: Number(form.get("dueDay")),
        initialBalance: String(form.get("initialBalance"))
      });
      setCreateOpen(false); setNotice("Cartão criado com sucesso."); notifyFinanceDataChanged();
    } catch (err) { setError(getErrorMessage(err)); }
    finally { setBusy(false); }
  }

  return <>
    <PageHeader title="Cartões" description="Limite e utilização dos seus cartões." action={<button className="primary-btn" onClick={() => { setError(""); setCreateOpen(true); }}>+ Novo cartão</button>} />
    {notice && <div className="alert success" role="status">{notice}<button className="alert-dismiss" onClick={() => setNotice("")}>×</button></div>}
    {error && <div className="alert error" role="alert">{error}</div>}
    <div className="cards-grid">{loading ? <Loading /> : items.length ? items.map(card => {
      const limit = card.creditCardSettings?.creditLimit ?? 0;
      const used = Math.max(0, card.calculatedBalance);
      const available = Math.max(0, limit - used);
      const percent = limit > 0 ? Math.min(100, used / limit * 100) : 0;
      return <article className="credit-card" key={card.id}><div className="credit-top"><span>{card.name}</span><span>Crédito</span></div>
        <div className="credit-bottom"><div><small>Utilizado</small><strong>{money(used)}</strong></div><div><small>Disponível</small><strong>{money(available)}</strong></div><div><small>Limite</small><strong>{money(limit)}</strong></div></div><div className="progress"><span style={{ width: `${percent}%` }} /></div>
        <div className="card-meta"><span>Fecha dia {card.creditCardSettings?.closingDay}</span><span>Vence dia {card.creditCardSettings?.dueDay}</span>{!card.isActive && <span className="badge">Inativo</span>}</div>
      </article>;
    }) : <div className="panel"><EmptyState title="Nenhum cartão" description="Cadastre seu cartão para acompanhar limites e faturas." action={<button className="primary-btn" onClick={() => setCreateOpen(true)}>+ Novo cartão</button>} /></div>}</div>
    {createOpen && <Modal title="Novo cartão" onClose={() => setCreateOpen(false)} onSubmit={createCard} busy={busy} error={error}>
      <label>Nome<input className="input" name="name" required maxLength={100} autoFocus /></label>
      <label>Limite<input className="input" name="creditLimit" type="number" min="0.01" step="0.01" required /></label>
      <label>Dia de fechamento<input className="input" name="closingDay" type="number" min="1" max="31" required /></label>
      <label>Dia de vencimento<input className="input" name="dueDay" type="number" min="1" max="31" required /></label>
      <label>Valor pendente anterior ao cadastro<input className="input" name="initialBalance" type="number" min="0" step="0.01" required defaultValue="0" /></label>
    </Modal>}
  </>;
}
