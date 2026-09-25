import { useEffect,useState } from "react";
import api, { getErrorMessage } from "../../services/api";
import type { Account } from "../../types";
import { PageHeader } from "../../components/PageHeader";
import Loading from "../../components/Loading";
import EmptyState from "../../components/EmptyState";

const money=(v:number|undefined)=>new Intl.NumberFormat("pt-BR",{style:"currency",currency:"BRL"}).format(v??0);
export default function Accounts(){
 const [items,setItems]=useState<Account[]>([]); const [loading,setLoading]=useState(true); const [error,setError]=useState("");
 useEffect(()=>{api.get<Account[]>("/accounts").then(r=>setItems(Array.isArray(r.data)?r.data:[])).catch(e=>setError(getErrorMessage(e))).finally(()=>setLoading(false))},[]);
 return <><PageHeader title="Contas" description="Veja seus saldos por conta."/><div className="panel">{error&&<div className="alert error">{error}</div>}{loading?<Loading/>:items.length?<div className="table-wrap"><table><thead><tr><th>Nome</th><th>Tipo</th><th>Status</th><th>Saldo</th></tr></thead><tbody>{items.map(a=><tr key={a.id}><td>{a.name}</td><td>{a.type}</td><td>{a.isActive===false?"Inativa":"Ativa"}</td><td className="money">{money(a.cachedBalance??a.balance)}</td></tr>)}</tbody></table></div>:<EmptyState title="Nenhuma conta" description="Cadastre sua primeira conta para começar."/>}</div></>;
}
