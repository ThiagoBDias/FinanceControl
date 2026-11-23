import { useCallback, useEffect, useState, type FormEvent } from "react";
import api, { getErrorMessage } from "../../services/api";
import { notifyFinanceDataChanged } from "../../services/financeEvents";
import { dateLabel, isCard, money, today } from "../../services/format";
import type { Account, Invoice, Transaction } from "../../types";
import PageHeader from "../../components/PageHeader";
import Loading from "../../components/Loading";
import EmptyState from "../../components/EmptyState";
import Modal from "../../components/Modal";

const isOpen = (invoice: Invoice) => String(invoice.status) === "1" || invoice.status === "Open";

export default function Invoices() {
  const [items, setItems] = useState<Invoice[]>([]);
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [notice, setNotice] = useState("");
  const [selection, setSelection] = useState<{ invoice: Invoice; action: "close" | "pay" } | null>(null);
  const [detail, setDetail] = useState<{ invoice: Invoice; entries?: Transaction[]; error?: string } | null>(null);
  const [busy, setBusy] = useState(false);

  const sources = accounts.filter((account) => account.isActive && !isCard(account.type));
  const load = useCallback(async () => {
    setError("");
    setLoading(true);
    try {
      const { data: accountList } = await api.get<Account[]>("/accounts");
      const responses = await Promise.all(accountList.filter((account) => isCard(account.type))
        .map((card) => api.get<Invoice[]>("/invoices", { params: { creditCardAccountId: card.id } })));
      setAccounts(accountList);
      setItems(responses.flatMap((response) => response.data).sort((a, b) => b.dueDate.localeCompare(a.dueDate)));
    } catch (err) { setError(getErrorMessage(err)); }
    finally { setLoading(false); }
  }, []);
  useEffect(() => { void load(); }, [load]);

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!selection) return;
    const { invoice, action } = selection;
    const form = new FormData(event.currentTarget);
    const data = { accountId: form.get("accountId"), amount: form.get("amount"), paymentDate: form.get("paymentDate") };
    setBusy(true);
    setError("");
    try {
      await api.post(`/invoices/${invoice.id}/${action === "close" ? "close" : "payments"}`, action === "pay" ? data : undefined);
      setNotice(action === "close" ? "Fatura fechada." : "Pagamento registrado.");
      setSelection(null);
      notifyFinanceDataChanged();
      await load();
    } catch (err) { setError(getErrorMessage(err)); }
    finally { setBusy(false); }
  }

  async function openDetails(invoice: Invoice) {
    setDetail({ invoice });
    try {
      const { data: entries } = await api.get<Transaction[]>("/transactions", { params: { invoiceId: invoice.id } });
      setDetail((current) => current?.invoice.id === invoice.id ? { invoice, entries } : current);
    } catch (err) {
      setDetail((current) => current?.invoice.id === invoice.id ? { invoice, error: getErrorMessage(err) } : current);
    }
  }

  return <>
    <PageHeader title="Faturas" description="Acompanhe compras, vencimentos e pagamentos por cartão." />
    {notice && <div className="alert success" role="status">{notice}</div>}
    {error && !selection && <div className="alert error" role="alert">{error} <button className="secondary-btn" onClick={() => void load()}>Tentar novamente</button></div>}
    <div className="panel">{loading ? <Loading /> : items.length ? <div className="table-wrap" tabIndex={0} aria-label="Faturas por cartão">
      <table><thead><tr><th>Cartão</th><th>Fechamento</th><th>Vencimento</th><th>Status</th><th>Total</th><th>Pago</th><th>Pendente</th><th>Ações</th></tr></thead>
        <tbody>{items.map((invoice) => {
          const status = invoice.pendingAmount === 0 && invoice.totalAmount > 0 ? "Paga"
            : invoice.paidAmount > 0 ? "Pagamento parcial" : isOpen(invoice) ? "Aberta" : "Fechada";
          return <tr key={invoice.id}>
            <td>{accounts.find((card) => card.id === invoice.creditCardAccountId)?.name ?? "Cartão"}{invoice.openingAmount > 0 && <small className="transaction-note">Saldo inicial</small>}</td>
            <td>{dateLabel(invoice.closingDate)}</td><td>{dateLabel(invoice.dueDate)}</td>
            <td><span className="badge">{status}</span></td><td className="money">{money(invoice.totalAmount)}</td>
            <td className="money">{money(invoice.paidAmount)}</td><td className="money">{money(invoice.pendingAmount)}</td>
            <td><div className="row-actions">
              {invoice.openingAmount === 0 && <button className="secondary-btn" onClick={() => void openDetails(invoice)}>Compras</button>}
              {isOpen(invoice) && <button className="secondary-btn" onClick={() => { setError(""); setSelection({ invoice, action: "close" }); }}>Fechar fatura</button>}
              {!isOpen(invoice) && invoice.pendingAmount > 0 && <button className="secondary-btn" onClick={() => { setError(""); setSelection({ invoice, action: "pay" }); }}>Pagar fatura</button>}
            </div></td>
          </tr>;
        })}</tbody>
      </table>
    </div> : !error && <EmptyState title="Nenhuma fatura" description="Registre uma compra no cartão para gerar a primeira fatura." />}</div>
    {selection && <Modal title={selection.action === "close" ? "Fechar fatura" : "Pagar fatura"} onClose={() => setSelection(null)}
      onSubmit={submit} busy={busy} error={error} submitLabel={selection.action === "close" ? "Confirmar fechamento" : "Registrar pagamento"}
      submitDisabled={selection.action === "pay" && !sources.length}>
      {selection.action === "close"
        ? <p className="form-hint">Após fechar, esta fatura não recebe novas compras. Total atual {money(selection.invoice.totalAmount)}.</p>
        : <>
          <p className="form-hint">Saldo pendente {money(selection.invoice.pendingAmount)}. Altere o valor para pagar parcialmente.</p>
          <label>Conta de pagamento<select className="input" name="accountId" required>
            <option value="">Selecione uma conta</option>{sources.map((account) => <option key={account.id} value={account.id}>{account.name}</option>)}
          </select></label>
          <label>Valor do pagamento<input className="input" name="amount" type="number" min="0.01" step="0.01"
            max={selection.invoice.pendingAmount} defaultValue={selection.invoice.pendingAmount.toFixed(2)} required /></label>
          <label>Data do pagamento<input className="input" name="paymentDate" type="date" defaultValue={today()} max={today()} required /></label>
        </>}
    </Modal>}
    {detail && <Modal title="Compras da fatura" onClose={() => setDetail(null)}>
      {detail.error ? <div role="alert" className="alert error">{detail.error}</div> : !detail.entries ? <Loading />
        : detail.entries.length ? detail.entries.map((entry) => <div className="category-row" key={entry.id}>
          <span>{entry.description}{entry.installmentCount && entry.installmentCount > 1 ? ` (${entry.installmentNumber}/${entry.installmentCount})` : ""}</span>
          <strong>{money(entry.amount)}</strong>
        </div>) : <EmptyState title="Sem compras" description="Nenhuma compra foi mantida nesta fatura." />}
    </Modal>}
  </>;
}
