import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import api, { getErrorMessage } from "../../services/api";
import { dateLabel, money, today } from "../../services/format";
import type { DashboardData } from "../../types";
import PageHeader from "../../components/PageHeader";
import StatCard from "../../components/StatCard";
import Loading from "../../components/Loading";
import EmptyState from "../../components/EmptyState";
import { onFinanceDataChanged } from "../../services/financeEvents";

const monthLabel = (year: number, month: number) => `${String(month).padStart(2, "0")}/${year}`;
const kinds: Record<string, { label: string; path: string }> = {
  bill: { label: "Conta a pagar", path: "/transactions" },
  invoice: { label: "Fatura", path: "/invoices" },
  debt: { label: "Dívida", path: "/debts" },
};

export default function Dashboard() {
  const [period, setPeriod] = useState(today().slice(0, 7));
  const [data, setData] = useState<DashboardData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [refresh, setRefresh] = useState(0);

  useEffect(() => onFinanceDataChanged(() => setRefresh((value) => value + 1)), []);
  useEffect(() => {
    const controller = new AbortController();
    async function load() {
      setLoading(true);
      setError("");
      const [year, month] = period.split("-");
      try {
        const { data: result } = await api.get<DashboardData>("/dashboard", { params: { year, month }, signal: controller.signal });
        setData(result);
      } catch (err) {
        if (!controller.signal.aborted) { setData(null); setError(getErrorMessage(err)); }
      } finally {
        if (!controller.signal.aborted) setLoading(false);
      }
    }
    void load();
    return () => controller.abort();
  }, [period, refresh]);

  const chartMax = Math.max(1, ...(data?.monthlyEvolution.flatMap((month) => [month.income, month.expenses]) ?? []));
  const currentDate = today();

  return <>
    <PageHeader title="Dashboard" action={<label className="period-control">Competência
      <input className="input" type="month" min="2000-01" max="2100-12" value={period}
        onChange={(event) => { if (event.target.validity.valid && event.target.value) setPeriod(event.target.value); }} />
    </label>} />
    {error && <div className="alert error" role="alert">{error} <button className="secondary-btn" onClick={() => setRefresh((value) => value + 1)}>Tentar novamente</button></div>}
    {loading ? <Loading /> : data && <>
      <div className="stats-grid">
        <StatCard label="Saldo em contas" value={money(data.cashBalance)} icon="R$" />
        <StatCard label="Receitas do mês" value={money(data.income)} icon="↑" tone="green" />
        <StatCard label="Despesas do mês" value={money(data.expenses)} icon="↓" tone="red" />
        <StatCard label="Resultado do mês" value={money(data.result)} icon="=" tone="blue" />
      </div>
      <div className="dashboard-grid">
        <section className="panel">
          <div className="panel-head"><h2>Evolução financeira</h2></div>
          <p className="chart-legend"><span>Receitas</span><span>Despesas</span></p>
          <div className="month-chart">
            {data.monthlyEvolution.map((month) => <div key={monthLabel(month.year, month.month)} className="month-chart-row">
              <span>{monthLabel(month.year, month.month)}</span>
              <div className="month-chart-bars">
                <div title={`Receitas ${money(month.income)}`}><span aria-hidden="true" style={{ width: `${100 * month.income / chartMax}%` }} /><strong aria-label={`Receitas ${money(month.income)}`}>{money(month.income)}</strong></div>
                <div title={`Despesas ${money(month.expenses)}`}><span aria-hidden="true" style={{ width: `${100 * month.expenses / chartMax}%` }} /><strong aria-label={`Despesas ${money(month.expenses)}`}>{money(month.expenses)}</strong></div>
              </div>
            </div>)}
          </div>
        </section>
        <section className="panel">
          <div className="panel-head"><h2>Saldos por conta</h2></div>
          {data.accounts.length ? <>
            {data.accounts.map((account) => <div className="category-row" key={account.accountId}>
              <Link to={`/transactions?accountId=${account.accountId}`}>{account.name}</Link>
              <strong className="money">{money(account.isLiability ? -account.balance : account.balance)}</strong>
            </div>)}
            <div className="category-row"><span>Saldo após compromissos no cartão</span><strong className="money">{money(data.netBalance)}</strong></div>
          </> : <EmptyState title="Nenhuma conta" description="Cadastre uma conta para acompanhar seu saldo." />}
        </section>
      </div>
      <div className="dashboard-grid">
        <section className="panel">
          <div className="panel-head"><h2>Vencidos e próximos 30 dias</h2></div>
          {data.upcoming.length ? data.upcoming.map((item, index) => <div className="category-row" key={`${item.kind}-${item.dueDate}-${index}`}>
            <Link to={kinds[item.kind].path}>{item.name}<small className="transaction-note">{kinds[item.kind].label} · {dateLabel(item.dueDate)}{item.dueDate < currentDate ? " · Vencido" : ""}</small></Link>
            <strong className="money">{money(item.amount)}</strong>
          </div>) : <EmptyState title="Nenhum vencimento próximo" description="Contas a pagar, faturas e parcelas pendentes aparecem aqui." />}
        </section>
        <section className="panel">
          <div className="panel-head"><h2>Projeção mensal de pagamentos</h2></div>
          {data.projectedCommitments.some((month) => month.amount > 0) ? data.projectedCommitments.map((month) =>
            <div className="category-row" key={monthLabel(month.year, month.month)}><span>{monthLabel(month.year, month.month)}</span>
              <strong className="money">{money(month.amount)}</strong></div>)
            : <EmptyState title="Sem compromissos projetados" description="Os próximos 12 meses aparecem conforme seus vencimentos." />}
        </section>
      </div>
      <div className="dashboard-grid">
        <section className="panel"><div className="panel-head"><h2>Despesas por categoria</h2></div>
          {data.expensesByCategory.length ? data.expensesByCategory.map((item, index) =>
            <div className="category-row" key={item.categoryId ?? index}><span>{item.categoryName}</span><strong>{money(item.amount)}</strong></div>)
            : <EmptyState title="Sem despesas no período" description="Os gastos por categoria aparecem aqui." />}
        </section>
        <section className="panel"><div className="panel-head"><h2>Orçamentos do mês</h2></div>
          {data.budgets.length ? data.budgets.map((budget) =>
            <div className="category-row" key={budget.categoryId}><Link to="/budgets">{budget.categoryName}</Link>
              <strong>{money(budget.spentAmount)} / {money(budget.limitAmount)}</strong></div>)
            : <EmptyState title="Nenhum orçamento" description="Defina limites mensais por categoria." />}
        </section>
      </div>
    </>}
  </>;
}
