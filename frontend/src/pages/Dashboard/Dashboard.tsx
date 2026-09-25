import { useEffect, useState } from "react";
import api from "../../services/api";
import type { DashboardData } from "../../types";
import { PageHeader } from "../../components/PageHeader";
import StatCard from "../../components/StatCard";
import Loading from "../../components/Loading";
import EmptyState from "../../components/EmptyState";

const money=(v:number|undefined)=>new Intl.NumberFormat("pt-BR",{style:"currency",currency:"BRL"}).format(v??0);

export default function Dashboard(){
 const [data,setData]=useState<DashboardData|null>(null); const [loading,setLoading]=useState(true); const [error,setError]=useState("");
 const now=new Date();
 useEffect(()=>{ api.get<DashboardData>(`/dashboard?year=${now.getFullYear()}&month=${now.getMonth()+1}`).then(r=>setData(r.data)).catch(()=>setError("Não foi possível carregar o dashboard.")).finally(()=>setLoading(false)); },[]);
 if(loading)return <Loading/>;
 const balance=data?.balance??data?.totalBalance??0, income=data?.income??data?.totalIncome??0, expenses=data?.expenses??data?.totalExpenses??0, net=data?.net??data?.result??income-expenses;
 return <><PageHeader title="Dashboard" description="Veja para onde seu dinheiro está indo."/>
 {error&&<div className="alert error">{error} Verifique se a API está rodando.</div>}
 <div className="stats-grid"><StatCard label="Saldo" value={money(balance)} icon="R$" tone="purple"/><StatCard label="Entradas" value={money(income)} icon="↑" tone="green"/><StatCard label="Gastos" value={money(expenses)} icon="↓" tone="red"/><StatCard label="Resultado" value={money(net)} icon="=" tone="blue"/></div>
 <div className="dashboard-grid">
  <section className="panel"><div className="panel-head"><h2>Gastos por categoria</h2></div>{data?.expensesByCategory?.length?<div className="category-list">{data.expensesByCategory.map((x,i)=><div className="category-row" key={i}><span>{x.name}</span><strong>{money(x.amount)}</strong></div>)}</div>:<EmptyState title="Sem dados ainda" description="Os gastos por categoria aparecerão aqui."/>}</section>
  <section className="panel"><div className="panel-head"><h2>Visão rápida</h2></div><div className="quick-list"><div><span>Contas</span><strong>{data?.accounts?.length??"—"}</strong></div><div><span>Lançamentos recentes</span><strong>{data?.recentTransactions?.length??"—"}</strong></div><div><span>Período</span><strong>{String(now.getMonth()+1).padStart(2,"0")}/{now.getFullYear()}</strong></div></div></section>
 </div></>;
}
