import { useCallback, useEffect, useMemo, useState, type FormEvent } from "react";
import { Link } from "react-router-dom";
import api, { getErrorMessage } from "../../services/api";
import { notifyFinanceDataChanged, onFinanceDataChanged } from "../../services/financeEvents";
import type { Account, Category, Transaction } from "../../types";
import PageHeader from "../../components/PageHeader";
import Loading from "../../components/Loading";
import EmptyState from "../../components/EmptyState";
import Modal from "../../components/Modal";

const money = (value: number) => new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" }).format(value);
const typeName: Record<string, string> = { "1": "Entrada", "2": "Despesa", "3": "Transferência enviada", "4": "Transferência recebida", Income: "Entrada", Expense: "Despesa", TransferIn: "Transferência recebida", TransferOut: "Transferência enviada" };
const dateLabel = (value: string) => new Date(`${value.slice(0, 10)}T00:00:00`).toLocaleDateString("pt-BR");

export default function Transactions() {
  const [items, setItems] = useState<Transaction[]>([]);
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [notice, setNotice] = useState("");
  const [createOpen, setCreateOpen] = useState(false);
  const [busy, setBusy] = useState(false);
  const [kind, setKind] = useState<"income" | "expense">("expense");
  const [accountId, setAccountId] = useState("");

  const activeAccounts = useMemo(() => accounts.filter((account) => account.isActive), [accounts]);
  const selectedAccount = activeAccounts.find((account) => account.id === accountId);
  const relevantCategories = useMemo(() => categories.filter((category) => category.isActive && String(category.type) === (kind === "income" ? "1" : "2")), [categories, kind]);

  const load = useCallback(async () => {
    setError("");
    try {
      const [transactions, accountList, categoryList] = await Promise.all([
        api.get<Transaction[]>("/transactions"), api.get<Account[]>("/accounts"), api.get<Category[]>("/categories")
      ]);
      setItems(Array.isArray(transactions.data) ? transactions.data : []);
      setAccounts(Array.isArray(accountList.data) ? accountList.data : []);
      setCategories(Array.isArray(categoryList.data) ? categoryList.data : []);
    } catch (err) { setError(getErrorMessage(err)); }
    finally { setLoading(false); }
  }, []);
  useEffect(() => { void load(); return onFinanceDataChanged(() => { void load(); }); }, [load]);

  async function createTransaction(event: FormEvent<HTMLFormElement>) {
    event.preventDefault(); setBusy(true); setError("");
    const form = new FormData(event.currentTarget);
    const categoryValue = String(form.get("categoryId") ?? "");
    const dueDateValue = String(form.get("dueDate") ?? "");
    const payload = {
      accountId: String(form.get("accountId")), amount: Number(form.get("amount")),
      competenceDate: String(form.get("competenceDate")), description: String(form.get("description")).trim(),
      categoryId: categoryValue || null, dueDate: dueDateValue || null
    };
    try {
      await api.post(`/transactions/${kind}`, payload);
      setCreateOpen(false); setNotice(kind === "income" ? "Entrada registrada com sucesso." : "Despesa registrada com sucesso."); notifyFinanceDataChanged();
    } catch (err) { setError(getErrorMessage(err)); }
    finally { setBusy(false); }
  }

  return <>
    <PageHeader title="Lançamentos" description="Tudo que entrou e saiu do seu dinheiro." action={<button className="primary-btn" onClick={() => { setError(""); setCreateOpen(true); if (!accountId && activeAccounts[0]) setAccountId(activeAccounts[0].id); }}>+ Novo lançamento</button>} />
    {notice && <div className="alert success" role="status">{notice}<button className="alert-dismiss" onClick={() => setNotice("")}>×</button></div>}
    {error && <div className="alert error" role="alert">{error}</div>}
    <div className="panel">{loading ? <Loading /> : items.length ? <div className="table-wrap"><table><thead><tr><th>Data</th><th>Tipo</th><th>Descrição</th><th>Categoria</th><th>Conta</th><th>Valor</th></tr></thead><tbody>
      {items.map(item => <tr key={item.id}><td>{dateLabel(item.competenceDate)}</td><td>{typeName[String(item.type)] ?? item.type}</td><td>{item.description || "—"}</td><td>{categories.find((category) => category.id === item.categoryId)?.name ?? "—"}</td><td>{accounts.find((account) => account.id === item.accountId)?.name ?? "Conta"}</td><td className="money">{money(item.amount)}</td></tr>)}
    </tbody></table></div> : <EmptyState title="Nenhum lançamento" description="Registre sua primeira entrada ou despesa para acompanhar seu saldo." action={<button className="primary-btn" onClick={() => { if (!accountId && activeAccounts[0]) setAccountId(activeAccounts[0].id); setCreateOpen(true); }}>+ Novo lançamento</button>} />}</div>
    {createOpen && <Modal title="Novo lançamento" onClose={() => setCreateOpen(false)} onSubmit={createTransaction} busy={busy} submitDisabled={!activeAccounts.length} error={error}>
      <label>Tipo<select className="input" value={kind} onChange={(event) => setKind(event.target.value as "income" | "expense")}><option value="income">Entrada</option><option value="expense">Despesa</option></select></label>
      {activeAccounts.length ? <label>Conta<select className="input" name="accountId" required value={accountId} onChange={(event) => setAccountId(event.target.value)}>{activeAccounts.map(account => <option key={account.id} value={account.id}>{account.name}{String(account.type) === "4" ? " · Cartão de crédito" : ""}</option>)}</select></label> : <div className="alert error">Crie uma conta ativa antes de registrar lançamentos. <Link to="/accounts">Ir para contas</Link></div>}
      <label>Categoria<select className="input" name="categoryId" defaultValue=""><option value="">Sem categoria</option>{relevantCategories.map(category => <option key={category.id} value={category.id}>{category.name}</option>)}</select></label>
      <label>Descrição<input className="input" name="description" required maxLength={200} /></label>
      <label>Valor<input className="input" name="amount" type="number" min="0.01" step="0.01" required /></label>
      <label>Data de competência<input className="input" name="competenceDate" type="date" required defaultValue={new Date().toISOString().slice(0, 10)} /></label>
      <label>Vencimento (opcional)<input className="input" name="dueDate" type="date" /></label>
      {!activeAccounts.length && <p className="form-hint">A API exige uma conta para registrar um lançamento.</p>}
    </Modal>}
  </>;
}
