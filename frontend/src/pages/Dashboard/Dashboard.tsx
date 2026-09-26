import { useCallback, useEffect, useState } from "react";
import api, { getErrorMessage } from "../../services/api";
import type { DashboardData } from "../../types";
import PageHeader from "../../components/PageHeader";
import StatCard from "../../components/StatCard";
import Loading from "../../components/Loading";
import EmptyState from "../../components/EmptyState";
import { onFinanceDataChanged } from "../../services/financeEvents";

const money = (value: number) => new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" }).format(value);
export default function Dashboard() {
  const [data, setData] = useState<DashboardData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const now = new Date();
  const load = useCallback(async () => {
    setError("");
    try { const { data: result } = await api.get<DashboardData>(`/dashboard?year=${now.getFullYear()}&month=${now.getMonth() + 1}`); setData(result); }
    catch (err) { setError(getErrorMessage(err)); }
    finally { setLoading(false); }
  }, [now.getFullYear(), now.getMonth()]);
  useEffect(() => { void load(); return onFinanceDataChanged(() => { void load(); }); }, [load]);
  return <><PageHeader title="Dashboard" description="Resumo da sua vida financeira no mês."/>
    {error && <div className="alert error">{error}</div>}{loading ? <Loading/> : data ? <>
      <div className="stats-grid"><StatCard label="Saldo total" value={money(data.netBalance)} icon="R$"/><StatCard label="Entradas" value={money(data.income)} icon="↑" tone="green"/><StatCard label="Despesas" value={money(data.expenses)} icon="↓" tone="red"/><StatCard label="Resultado do mês" value={money(data.result)} icon="=" tone="blue"/></div>
      <div className="dashboard-grid">
        <section className="panel"><div className="panel-head"><h2>Principais gastos por categoria</h2></div>{data.expensesByCategory.length ? data.expensesByCategory.map((item, index) => <div className="category-row" key={item.categoryId ?? `${item.categoryName}-${index}`}><span>{item.categoryName}</span><strong>{money(item.amount)}</strong></div>) : <EmptyState title="Sem despesas no período" description="Os gastos por categoria aparecerão aqui."/>}</section>
        <section className="panel"><div className="panel-head"><h2>Visão geral</h2></div><div className="quick-list"><div><span>Contas</span><strong>{data.accounts.length}</strong></div><div><span>Cartões</span><strong>{data.creditCards.length}</strong></div><div><span>Faturas</span><strong>{data.invoices.length}</strong></div><div><span>Orçamentos</span><strong>{data.budgets.length}</strong></div></div></section>
      </div>
      <div className="dashboard-grid"><section className="panel"><div className="panel-head"><h2>Cartões</h2></div>{data.creditCards.length ? data.creditCards.map(card => <div className="category-row" key={card.accountId}><span>{card.name}</span><strong>{money(card.usedAmount)} / {money(card.creditLimit)}</strong></div>) : <EmptyState title="Nenhum cartão" description="Seus cartões aparecerão aqui."/>}</section>
        <section className="panel"><div className="panel-head"><h2>Orçamentos do mês</h2></div>{data.budgets.length ? data.budgets.map(budget => <div className="category-row" key={budget.categoryId}><span>{budget.categoryName}</span><strong>{money(budget.spentAmount)} / {money(budget.limitAmount)}</strong></div>) : <EmptyState title="Nenhum orçamento" description="Os orçamentos do mês aparecerão aqui."/>}</section></div>
    </> : <div className="panel"><EmptyState title="Resumo indisponível" description="A API não retornou dados do dashboard."/></div>}
  </>;
}
