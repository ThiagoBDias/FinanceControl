import { useCallback, useEffect, useMemo, useState, type FormEvent } from "react";
import { Link } from "react-router-dom";
import api, { getErrorMessage } from "../../services/api";
import { notifyFinanceDataChanged, onFinanceDataChanged } from "../../services/financeEvents";
import type { Budget, Category } from "../../types";
import PageHeader from "../../components/PageHeader";
import EmptyState from "../../components/EmptyState";
import Loading from "../../components/Loading";
import Modal from "../../components/Modal";

const money = (value: number) => new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" }).format(value);
const currentPeriod = new Date().toISOString().slice(0, 7);

export default function Budgets() {
  const [items, setItems] = useState<Budget[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [period, setPeriod] = useState(currentPeriod);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [notice, setNotice] = useState("");
  const [createOpen, setCreateOpen] = useState(false);
  const [editing, setEditing] = useState<Budget | null>(null);
  const [busy, setBusy] = useState(false);
  const [year, month] = useMemo(() => period.split("-").map(Number), [period]);
  const expenseCategories = categories.filter((category) => category.isActive && String(category.type) === "2");

  const load = useCallback(async () => {
    setError("");
    try {
      const [budgets, categoryResponse] = await Promise.all([
        api.get<Budget[]>(`/budgets?year=${year}&month=${month}`), api.get<Category[]>("/categories")
      ]);
      setItems(Array.isArray(budgets.data) ? budgets.data : []);
      setCategories(Array.isArray(categoryResponse.data) ? categoryResponse.data : []);
    } catch (err) { setError(getErrorMessage(err)); }
    finally { setLoading(false); }
  }, [year, month]);
  useEffect(() => { void load(); return onFinanceDataChanged(() => { void load(); }); }, [load]);

  async function createBudget(event: FormEvent<HTMLFormElement>) {
    event.preventDefault(); setBusy(true); setError("");
    const form = new FormData(event.currentTarget);
    try {
      await api.post("/budgets", { categoryId: String(form.get("categoryId")), year, month, limitAmount: String(form.get("limitAmount")) });
      setCreateOpen(false); setNotice("Orçamento criado com sucesso."); notifyFinanceDataChanged();
    } catch (err) { setError(getErrorMessage(err)); }
    finally { setBusy(false); }
  }
  async function updateBudget(event: FormEvent<HTMLFormElement>) {
    event.preventDefault(); if (!editing) return; setBusy(true); setError("");
    const form = new FormData(event.currentTarget);
    try {
      await api.put(`/budgets/${editing.id}`, { limitAmount: String(form.get("limitAmount")) });
      setEditing(null); setNotice("Limite atualizado."); notifyFinanceDataChanged();
    } catch (err) { setError(getErrorMessage(err)); }
    finally { setBusy(false); }
  }

  return <>
    <PageHeader title="Orçamento" description="Compare o limite planejado com os gastos do mês." action={<div className="header-actions"><label className="period-control">Mês<input className="input" type="month" value={period} onChange={(event) => setPeriod(event.target.value)} /></label><button className="primary-btn" onClick={() => { setError(""); setCreateOpen(true); }}>+ Novo orçamento</button></div>} />
    {notice && <div className="alert success" role="status">{notice}<button className="alert-dismiss" onClick={() => setNotice("")}>×</button></div>}
    {error && <div className="alert error" role="alert">{error}</div>}
    <div className="panel">{loading ? <Loading /> : items.length ? <div className="table-wrap"><table><thead><tr><th>Categoria</th><th>Limite</th><th>Gasto</th><th>Restante</th><th>Progresso</th><th>Situação</th><th>Ações</th></tr></thead><tbody>{items.map(budget => {
      const percent = budget.limitAmount > 0 ? budget.spentAmount / budget.limitAmount * 100 : 0;
      return <tr key={budget.id}><td>{budget.categoryName}</td><td className="money">{money(budget.limitAmount)}</td><td className="money">{money(budget.spentAmount)}</td><td className="money">{money(budget.remainingAmount)}</td><td><div className={`budget-progress${budget.isOverLimit ? " over" : ""}`}><span style={{ width: `${Math.min(100, percent)}%` }} /></div>{Math.round(percent)}%</td><td><span className={`badge${budget.isOverLimit ? " badge-danger" : ""}`}>{budget.isOverLimit ? "Acima do limite" : "Dentro do limite"}</span></td><td><button className="secondary-btn" onClick={() => { setError(""); setEditing(budget); }}>Editar limite</button></td></tr>;
    })}</tbody></table></div> : <EmptyState title="Nenhum orçamento" description={`Ainda não há orçamento para ${String(month).padStart(2, "0")}/${year}.`} action={expenseCategories.length ? <button className="primary-btn" onClick={() => setCreateOpen(true)}>+ Novo orçamento</button> : <Link className="primary-btn inline-btn" to="/categories">Criar categoria de despesa</Link>} />}</div>
    {createOpen && <Modal title="Novo orçamento" onClose={() => setCreateOpen(false)} onSubmit={createBudget} busy={busy} submitDisabled={!expenseCategories.length} error={error}>
      <label>Categoria de despesa<select className="input" name="categoryId" required defaultValue="">{!expenseCategories.length && <option value="">Nenhuma categoria disponível</option>}{expenseCategories.map(category => <option key={category.id} value={category.id}>{category.name}</option>)}</select></label>
      <label>Mês<input className="input" type="month" value={period} onChange={(event) => setPeriod(event.target.value)} required /></label>
      <label>Limite<input className="input" name="limitAmount" type="number" min="0.01" step="0.01" required /></label>
      {!expenseCategories.length && <p className="form-hint">Crie primeiro uma categoria do tipo Despesa.</p>}
    </Modal>}
    {editing && <Modal title={`Editar orçamento · ${editing.categoryName}`} onClose={() => setEditing(null)} onSubmit={updateBudget} busy={busy} error={error}>
      <p className="form-hint">O orçamento existente não pode mudar de categoria ou período.</p>
      <label>Novo limite<input className="input" name="limitAmount" type="number" min="0.01" step="0.01" required defaultValue={editing.limitAmount} autoFocus /></label>
    </Modal>}
  </>;
}
