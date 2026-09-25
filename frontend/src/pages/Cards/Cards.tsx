import { useEffect,useState } from "react";
import api from "../../services/api";
import type { Account } from "../../types";
import { PageHeader } from "../../components/PageHeader";
import EmptyState from "../../components/EmptyState";
import Loading from "../../components/Loading";
const money=(v:number)=>new Intl.NumberFormat("pt-BR",{style:"currency",currency:"BRL"}).format(v||0);
export default function Cards(){
 const[items,setItems]=useState<Account[]>([]);const[loading,setLoading]=useState(true);
 useEffect(() => {
  api.get<Account[]>("/accounts")
    .then(r => {
      const accounts = Array.isArray(r.data) ? r.data : [];
      setItems(accounts.filter(a => String(a.type).toLowerCase().includes("credit")));
    })
    .catch(() => setItems([]))
    .finally(() => setLoading(false));
}, []);
 return <><PageHeader title="Cartões" description="Limite, utilização e informações dos seus cartões."/><div className="cards-grid">{loading?<Loading/>:items.length?items.map(c=>{const limit=c.creditCardSettings?.creditLimit??0;const used=Math.max(0,-(c.cachedBalance??0));const pct=limit?Math.min(100,used/limit*100):0;return <div className="credit-card" key={c.id}><div className="credit-top"><span>{c.name}</span><span>VISA</span></div><div className="credit-number">•••• •••• ••••</div><div className="credit-bottom"><div><small>Utilizado</small><strong>{money(used)}</strong></div><div><small>Limite</small><strong>{money(limit)}</strong></div></div><div className="progress"><span style={{width:`${pct}%`}}/></div></div>}):<div className="panel"><EmptyState title="Nenhum cartão" description="Seus cartões de crédito aparecerão aqui."/></div>}</div></>;
}
