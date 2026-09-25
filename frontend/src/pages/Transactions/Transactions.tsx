import { useEffect,useState } from "react";
import api, { getErrorMessage } from "../../services/api";
import type { Transaction } from "../../types";
import { PageHeader } from "../../components/PageHeader";
import Loading from "../../components/Loading";
import EmptyState from "../../components/EmptyState";
const money=(v:number)=>new Intl.NumberFormat("pt-BR",{style:"currency",currency:"BRL"}).format(v);
export default function Transactions(){
 const [items,setItems]=useState<Transaction[]>([]);const[loading,setLoading]=useState(true);const[error,setError]=useState("");
 useEffect(()=>{api.get<Transaction[]>("/transactions").then(r=>setItems(Array.isArray(r.data)?r.data:[])).catch(e=>setError(getErrorMessage(e))).finally(()=>setLoading(false))},[]);
 return <><PageHeader title="Lançamentos" description="Tudo que entrou e saiu do seu dinheiro."/><div className="panel">{error&&<div className="alert error">{error}</div>}{loading?<Loading/>:items.length?<div className="table-wrap"><table><thead><tr><th>Data</th><th>Tipo</th><th>Descrição</th><th>Valor</th></tr></thead><tbody>{items.filter(x=>!x.isDeleted).map(x=><tr key={x.id}><td>{new Date(x.paymentDate).toLocaleDateString("pt-BR")}</td><td>{x.type}</td><td>{x.description||"—"}</td><td className="money">{money(x.amount)}</td></tr>)}</tbody></table></div>:<EmptyState title="Nenhum lançamento" description="Seus lançamentos aparecerão aqui."/>}</div></>;
}
