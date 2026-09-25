import { useEffect,useState } from "react";
import api from "../../services/api";
import type { Invoice } from "../../types";
import { PageHeader } from "../../components/PageHeader";
import Loading from "../../components/Loading";
import EmptyState from "../../components/EmptyState";
const money=(v:number)=>new Intl.NumberFormat("pt-BR",{style:"currency",currency:"BRL"}).format(v||0);
export default function Invoices(){
 const[items,setItems]=useState<Invoice[]>([]);const[loading,setLoading]=useState(true);
 useEffect(()=>{api.get<Invoice[]>("/invoices").then(r=>setItems(Array.isArray(r.data)?r.data:[])).catch(()=>setItems([])).finally(()=>setLoading(false))},[]);
 return <><PageHeader title="Faturas" description="Visualize os valores das suas faturas e seus vencimentos."/><div className="panel">{loading?<Loading/>:items.length?<div className="table-wrap"><table><thead><tr><th>Período</th><th>Fechamento</th><th>Vencimento</th><th>Status</th><th>Pago</th><th>Total</th></tr></thead><tbody>{items.map(i=><tr key={i.id}><td>{String(i.periodMonth).padStart(2,"0")}/{i.periodYear}</td><td>{new Date(i.closingDate).toLocaleDateString("pt-BR")}</td><td>{new Date(i.dueDate).toLocaleDateString("pt-BR")}</td><td><span className="badge">{i.status}</span></td><td>{money(i.paidAmount)}</td><td>{money(i.totalAmount)}</td></tr>)}</tbody></table></div>:<EmptyState title="Nenhuma fatura" description="As faturas dos seus cartões aparecerão aqui."/>}</div></>;
}
