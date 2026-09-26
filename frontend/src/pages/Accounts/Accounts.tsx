import { useCallback, useEffect, useState, type FormEvent } from "react";
import api, { getErrorMessage } from "../../services/api";
import { notifyFinanceDataChanged, onFinanceDataChanged } from "../../services/financeEvents";
import type { Account } from "../../types";
import PageHeader from "../../components/PageHeader";
import Loading from "../../components/Loading";
import EmptyState from "../../components/EmptyState";
import Modal from "../../components/Modal";

const money = (value: number) => new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" }).format(value);
const accountType: Record<string, string> = { "1": "Conta corrente", "2": "Poupança", "3": "Dinheiro", "4": "Cartão de crédito", "5": "Investimento" };

export default function Accounts() {
  const [items, setItems] = useState<Account[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [notice, setNotice] = useState("");
  const [editing, setEditing] = useState<Account | null>(null);
  const [createOpen, setCreateOpen] = useState(false);
  const [busy, setBusy] = useState(false);

  const load = useCallback(async () => {
    setError("");
    try { const { data } = await api.get<Account[]>("/accounts"); setItems(Array.isArray(data) ? data : []); }
    catch (err) { setError(getErrorMessage(err)); }
    finally { setLoading(false); }
  }, []);
  useEffect(() => { void load(); return onFinanceDataChanged(() => { void load(); }); }, [load]);

  async function createAccount(event: FormEvent<HTMLFormElement>) {
    event.preventDefault(); setBusy(true); setError("");
    const form = new FormData(event.currentTarget);
    try {
      await api.post("/accounts", { name: String(form.get("name")).trim(), type: Number(form.get("type")), initialBalance: Number(form.get("initialBalance")) });
      setEditing(null); setNotice("Conta criada com sucesso."); notifyFinanceDataChanged();
    } catch (err) { setError(getErrorMessage(err)); }
    finally { setBusy(false); }
  }
  async function renameAccount(event: FormEvent<HTMLFormElement>) {
    event.preventDefault(); if (!editing) return; setBusy(true); setError("");
    const form = new FormData(event.currentTarget);
    try {
      await api.put(`/accounts/${editing.id}`, { name: String(form.get("name")).trim() });
      setEditing(null); setNotice("Conta atualizada com sucesso."); notifyFinanceDataChanged();
    } catch (err) { setError(getErrorMessage(err)); }
    finally { setBusy(false); }
  }
  async function deactivate(account: Account) {
    if (!window.confirm(`Desativar a conta “${account.name}”?`)) return;
    setError("");
    try { await api.post(`/accounts/${account.id}/deactivate`); setNotice("Conta desativada."); notifyFinanceDataChanged(); }
    catch (err) { setError(getErrorMessage(err)); }
  }

  return <>
    <PageHeader title="Contas" description="Veja seus saldos por conta." action={<button className="primary-btn" onClick={() => { setError(""); setEditing(null); setCreateOpen(true); }}>+ Nova conta</button>} />
    {notice && <div className="alert success" role="status">{notice}<button className="alert-dismiss" onClick={() => setNotice("")}>×</button></div>}
    {error && <div className="alert error" role="alert">{error}</div>}
    <div className="panel">{loading ? <Loading /> : items.length ? <div className="table-wrap"><table><thead><tr><th>Nome</th><th>Tipo</th><th>Saldo inicial</th><th>Saldo atual</th><th>Status</th><th>Ações</th></tr></thead><tbody>{items.map(account =>
      <tr key={account.id}><td>{account.name}</td><td>{accountType[String(account.type)] ?? account.type}</td><td className="money">{money(account.initialBalance)}</td><td className="money">{money(account.calculatedBalance)}</td><td><span className="badge">{account.isActive ? "Ativa" : "Inativa"}</span></td><td><div className="row-actions"><button className="secondary-btn" onClick={() => { setError(""); setEditing(account); }}>Editar</button>{account.isActive && <button className="danger-btn" onClick={() => void deactivate(account)}>Desativar</button>}</div></td></tr>)}</tbody></table></div> : <EmptyState title="Nenhuma conta" description="Cadastre sua primeira conta para começar." action={<button className="primary-btn" onClick={() => setCreateOpen(true)}>+ Nova conta</button>} />}</div>
    {createOpen && <Modal title="Nova conta" onClose={() => setCreateOpen(false)} onSubmit={createAccount} busy={busy} error={error}>
      <label>Nome<input className="input" name="name" required maxLength={100} autoFocus /></label>
      <label>Tipo<select className="input" name="type" required defaultValue="1"><option value="1">Conta corrente</option><option value="2">Poupança</option><option value="3">Dinheiro</option></select></label>
      <label>Saldo inicial<input className="input" name="initialBalance" type="number" min="0" step="0.01" required defaultValue="0" /></label>
    </Modal>}
    {editing && <Modal title="Editar conta" onClose={() => setEditing(null)} onSubmit={renameAccount} busy={busy} error={error}>
      <label>Nome<input className="input" name="name" required maxLength={100} defaultValue={editing.name} autoFocus /></label>
      <p className="form-hint">Tipo e saldo inicial não podem ser alterados após a criação.</p>
    </Modal>}
  </>;
}
