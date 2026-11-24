import { useCallback, useEffect, useState, type FormEvent } from "react";
import { Link, useSearchParams } from "react-router-dom";
import api, { getErrorMessage } from "../../services/api";
import { notifyFinanceDataChanged } from "../../services/financeEvents";
import { dateLabel, isCard, money, today } from "../../services/format";
import type { Account, Category, Transaction } from "../../types";
import PageHeader from "../../components/PageHeader";
import Loading from "../../components/Loading";
import EmptyState from "../../components/EmptyState";
import Modal from "../../components/Modal";

const typeNames: Record<string, string> = {
  "1": "Receita", "2": "Despesa", "3": "Transferência enviada", "4": "Transferência recebida",
};
type Mode = "income" | "expense" | "transfer";

export default function Transactions() {
  const [filters, setFilters] = useSearchParams();
  const accountFilter = filters.get("accountId") ?? "";
  const [items, setItems] = useState<Transaction[]>([]);
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [notice, setNotice] = useState("");
  const [createOpen, setCreateOpen] = useState(false);
  const [settling, setSettling] = useState<Transaction | null>(null);
  const [busy, setBusy] = useState(false);
  const [kind, setKind] = useState<Mode>("expense");
  const [accountId, setAccountId] = useState("");
  const [scheduled, setScheduled] = useState(false);

  const eligibleAccounts = accounts.filter((account) => account.isActive && (kind === "expense" || !isCard(account.type)));
  const selectedAccount = eligibleAccounts.find((account) => account.id === accountId) ?? eligibleAccounts[0];
  const creditPurchase = kind === "expense" && !!selectedAccount && isCard(selectedAccount.type);
  const relevantCategories = categories.filter((category) => category.isActive && String(category.type) === (kind === "income" ? "1" : "2"));

  const load = useCallback(async (signal?: AbortSignal) => {
    setLoading(true);
    setError("");
    try {
      const [transactions, accountList, categoryList] = await Promise.all([
        api.get<Transaction[]>("/transactions", { params: { accountId: accountFilter || undefined }, signal }),
        api.get<Account[]>("/accounts", { signal }),
        api.get<Category[]>("/categories", { signal }),
      ]);
      setItems(transactions.data);
      setAccounts(accountList.data);
      setCategories(categoryList.data);
    } catch (err) {
      if (!signal?.aborted) setError(getErrorMessage(err));
    } finally {
      if (!signal?.aborted) setLoading(false);
    }
  }, [accountFilter]);

  useEffect(() => {
    const controller = new AbortController();
    void load(controller.signal);
    return () => controller.abort();
  }, [load]);

  function openCreate(mode: Mode) {
    setKind(mode);
    setAccountId(accountFilter);
    setScheduled(false);
    setError("");
    setCreateOpen(true);
  }

  async function mutate(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    const read = (name: string) => String(form.get(name) ?? "");
    setBusy(true);
    setError("");
    try {
      const date = read("date");
      if (settling) {
        await api.post(`/transactions/${settling.id}/settle`, { paymentDate: date });
        setSettling(null);
        setNotice("Pagamento registrado.");
      } else if (kind === "transfer") {
        await api.post("/transfers", {
          sourceAccountId: read("accountId"), destinationAccountId: read("destination"), amount: read("amount"),
          transferDate: date, description: read("description").trim(),
        });
        setCreateOpen(false);
        setNotice("Transferência registrada.");
      } else {
        await api.post(`/transactions/${kind}`, {
          accountId: read("accountId"), amount: read("amount"), competenceDate: date,
          description: read("description").trim(), categoryId: read("categoryId") || null,
          dueDate: read("dueDate") || null, scheduled: !creditPurchase && kind === "expense" && scheduled,
          installmentCount: creditPurchase ? Number(read("installmentCount")) : 1,
        });
        setCreateOpen(false);
        setNotice("Lançamento registrado.");
      }
      notifyFinanceDataChanged();
      await load();
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setBusy(false);
    }
  }

  return <>
    <PageHeader title="Lançamentos" action={<div className="row-actions">
      <button className="secondary-btn" onClick={() => openCreate("transfer")}>Transferir</button>
      <button className="primary-btn" onClick={() => openCreate("expense")}>Novo lançamento</button>
    </div>} />
    {notice && <div className="alert success" role="status">{notice}<button className="alert-dismiss" aria-label="Dispensar mensagem" onClick={() => setNotice("")}>×</button></div>}
    {error && !createOpen && !settling && <div className="alert error" role="alert">{error} <button className="secondary-btn" onClick={() => void load()}>Tentar novamente</button></div>}
    <div className="panel">
      <label className="period-control account-filter">Filtrar por conta
        <select className="input" value={accountFilter} onChange={(event) => setFilters(event.target.value ? { accountId: event.target.value } : {})}>
          <option value="">Todas as contas</option>
          {accounts.map((account) => <option key={account.id} value={account.id}>{account.name}</option>)}
        </select>
      </label>
      {loading ? <Loading /> : items.length ? <div className="table-wrap" tabIndex={0} aria-label="Histórico de lançamentos">
        <table><thead><tr><th>Competência</th><th>Tipo</th><th>Descrição</th><th>Conta</th><th>Valor</th><th>Pagamento</th></tr></thead>
          <tbody>{items.map((item) => <tr key={item.id}>
            <td>{dateLabel(item.competenceDate)}</td><td>{typeNames[String(item.type)] ?? item.type}</td>
            <td>{item.description}{item.installmentCount && item.installmentCount > 1 && <small className="transaction-note">Parcela {item.installmentNumber} de {item.installmentCount}</small>}
              {item.categoryId && <small className="transaction-note">{categories.find((category) => category.id === item.categoryId)?.name}</small>}</td>
            <td>{accounts.find((account) => account.id === item.accountId)?.name ?? "Conta"}</td>
            <td className="money">{money(item.amount)}</td>
            <td>{item.isPending ? <button className="secondary-btn" onClick={() => { setError(""); setSettling(item); }}>Registrar pagamento</button> : item.paymentDate ? dateLabel(item.paymentDate) : "—"}</td>
          </tr>)}</tbody>
        </table>
      </div> : !error && <EmptyState title="Nenhum lançamento" description="Os lançamentos da conta selecionada aparecem aqui." />}
    </div>
    {createOpen && <Modal title={kind === "transfer" ? "Transferir entre contas" : "Novo lançamento"} onClose={() => setCreateOpen(false)}
      onSubmit={mutate} busy={busy} error={error} submitDisabled={!selectedAccount || (kind === "transfer" && eligibleAccounts.length < 2)}>
      {kind !== "transfer" && <label>Tipo<select className="input" value={kind} onChange={(event) => {
        setKind(event.target.value as Mode); setScheduled(false);
      }}><option value="expense">Despesa</option><option value="income">Receita</option></select></label>}
      {selectedAccount ? <label>{kind === "transfer" ? "Conta de origem" : "Conta"}<select className="input" name="accountId"
        value={selectedAccount.id} onChange={(event) => { setAccountId(event.target.value); setScheduled(false); }} required>
        {eligibleAccounts.map((account) => <option key={account.id} value={account.id}>{account.name}</option>)}
      </select></label> : <p className="form-hint">Cadastre uma <Link to="/accounts">conta ativa</Link> para prosseguir.</p>}
      {kind === "transfer" && <label>Conta de destino<select className="input" name="destination" key={selectedAccount?.id} required>
        <option value="">Selecione a conta</option>
        {eligibleAccounts.filter((a) => a.id !== selectedAccount?.id).map((account) => <option key={account.id} value={account.id}>{account.name}</option>)}
      </select></label>}
      {kind !== "transfer" && <label>Categoria<select className="input" name="categoryId" key={kind}>
        <option value="">Sem categoria</option>
        {relevantCategories.map((category) => <option key={category.id} value={category.id}>{category.name}</option>)}
      </select></label>}
      <label>Descrição<input className="input" name="description" required={kind !== "transfer"} maxLength={200} /></label>
      <label>Valor<input className="input" name="amount" type="number" min="0.01" step="0.01" required /></label>
      <label>{kind === "transfer" ? "Data da transferência" : "Data de competência"}<input className="input" name="date"
        type="date" max={kind === "transfer" ? today() : undefined} required defaultValue={today()} /></label>
      {creditPurchase && <label>Parcelas<input className="input" name="installmentCount" type="number" min="1" max="60" defaultValue="1" required /></label>}
      {!creditPurchase && kind === "expense" && <>
        <label className="checkbox-label"><input type="checkbox" checked={scheduled} onChange={(event) => setScheduled(event.target.checked)} /> Conta a pagar</label>
        {scheduled && <label>Vencimento<input className="input" name="dueDate" type="date" required /></label>}
      </>}
    </Modal>}
    {settling && <Modal title="Registrar pagamento" onClose={() => setSettling(null)} onSubmit={mutate} busy={busy} error={error}>
      <p className="form-hint">{settling.description} · {money(settling.amount)}</p>
      <label>Data do pagamento<input className="input" name="date" type="date" max={today()} defaultValue={today()} required /></label>
    </Modal>}
  </>;
}
