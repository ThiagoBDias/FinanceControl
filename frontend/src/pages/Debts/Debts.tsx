import { useCallback, useEffect, useState, type FormEvent } from "react";
import api, { getErrorMessage } from "../../services/api";
import { dateLabel, isCard, money, today } from "../../services/format";
import { notifyFinanceDataChanged } from "../../services/financeEvents";
import type { Account, Category, Debt, DebtInstallment } from "../../types";
import PageHeader from "../../components/PageHeader";
import EmptyState from "../../components/EmptyState";
import Loading from "../../components/Loading";
import Modal from "../../components/Modal";

export default function Debts() {
  const [debts, setDebts] = useState<Debt[]>([]);
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [creating, setCreating] = useState(false);
  const [payment, setPayment] = useState<{ debt: Debt; installment: DebtInstallment } | null>(null);
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState(false);
  const [notice, setNotice] = useState("");
  const [error, setError] = useState("");

  const load = useCallback(async () => {
    setLoading(true);
    setError("");
    try {
      const [debtList, accountList, categoryList] = await Promise.all([
        api.get<Debt[]>("/debts"), api.get<Account[]>("/accounts"), api.get<Category[]>("/categories"),
      ]);
      setDebts(debtList.data);
      setAccounts(accountList.data.filter((account) => account.isActive && !isCard(account.type)));
      setCategories(categoryList.data.filter((category) => category.isActive && String(category.type) === "2"));
    } catch (err) { setError(getErrorMessage(err)); }
    finally { setLoading(false); }
  }, []);
  useEffect(() => { void load(); }, [load]);

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    setBusy(true);
    setError("");
    try {
      if (payment) {
        await api.post(`/debts/${payment.debt.id}/installments/${payment.installment.number}/pay`,
          { accountId: form.get("accountId"), paymentDate: form.get("paymentDate") });
        setNotice("Parcela paga.");
      } else {
        await api.post("/debts", { name: form.get("name"), totalAmount: form.get("totalAmount"),
          installmentCount: Number(form.get("installmentCount")), firstDueDate: form.get("firstDueDate"),
          categoryId: form.get("categoryId") || null });
        setNotice("Dívida cadastrada.");
      }
      setPayment(null);
      setCreating(false);
      notifyFinanceDataChanged();
      await load();
    } catch (err) { setError(getErrorMessage(err)); }
    finally { setBusy(false); }
  }

  return <>
    <PageHeader title="Dívidas" description="Controle parcelamentos, financiamentos e próximos vencimentos."
      action={<button className="primary-btn" onClick={() => { setError(""); setCreating(true); }}>Nova dívida</button>} />
    {error && !creating && !payment && <div className="alert error" role="alert">{error} <button className="secondary-btn" onClick={() => void load()}>Tentar novamente</button></div>}
    {notice && <div className="alert success" role="status">{notice}</div>}
    {loading ? <Loading /> : debts.length ? <div className="panel debt-list">
      {debts.map((debt) => <details key={debt.id}>
        <summary><span>{debt.name}</span><strong>{money(debt.pendingAmount)} em aberto</strong></summary>
        <p className="form-hint">Total contratado {money(debt.totalAmount)}</p>
        <div className="table-wrap" tabIndex={0} aria-label={`Parcelas de ${debt.name}`}>
          <table><thead><tr><th>Parcela</th><th>Vencimento</th><th>Valor</th><th>Pagamento</th></tr></thead>
            <tbody>{debt.installments.map((installment) => <tr key={installment.number}>
              <td>{installment.number}</td><td>{dateLabel(installment.dueDate)}</td><td className="money">{money(installment.amount)}</td>
              <td>{installment.paymentDate ? `Paga em ${dateLabel(installment.paymentDate)}`
                : <button className="secondary-btn" onClick={() => { setError(""); setPayment({ debt, installment }); }}>Registrar pagamento</button>}</td>
            </tr>)}</tbody>
          </table>
        </div>
      </details>)}
    </div> : !error && <div className="panel"><EmptyState title="Nenhuma dívida cadastrada" description="Cadastre um financiamento ou outro compromisso parcelado." /></div>}
    {creating && <Modal title="Nova dívida" onClose={() => setCreating(false)} onSubmit={submit} busy={busy} error={error}>
      <label>Nome<input className="input" name="name" required maxLength={100} /></label>
      <label>Total a pagar, incluindo juros<input className="input" name="totalAmount" type="number" min="0.01" step="0.01" required /></label>
      <label>Quantidade de parcelas<input className="input" name="installmentCount" type="number" min="1" max="600" defaultValue="1" required /></label>
      <label>Primeiro vencimento<input className="input" name="firstDueDate" type="date" required defaultValue={today()} /></label>
      <label>Categoria<select className="input" name="categoryId"><option value="">Sem categoria</option>
        {categories.map((category) => <option key={category.id} value={category.id}>{category.name}</option>)}</select></label>
    </Modal>}
    {payment && <Modal title="Pagar parcela" onClose={() => setPayment(null)} onSubmit={submit} busy={busy} error={error} submitDisabled={!accounts.length}>
      <p className="form-hint">{payment.debt.name} · parcela {payment.installment.number} · {money(payment.installment.amount)}</p>
      <label>Conta de pagamento<select className="input" name="accountId" required><option value="">Selecione uma conta</option>
        {accounts.map((account) => <option key={account.id} value={account.id}>{account.name}</option>)}</select></label>
      <label>Data do pagamento<input className="input" name="paymentDate" type="date" max={today()} defaultValue={today()} required /></label>
    </Modal>}
  </>;
}
