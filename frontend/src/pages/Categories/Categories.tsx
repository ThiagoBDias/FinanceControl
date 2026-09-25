import { useEffect,useState } from "react";
import api from "../../services/api";
import type { Category } from "../../types";
import { PageHeader } from "../../components/PageHeader";
import Loading from "../../components/Loading";
import EmptyState from "../../components/EmptyState";
export default function Categories(){
 const[items,setItems]=useState<Category[]>([]);const[loading,setLoading]=useState(true);const[error,setError]=useState("");
 useEffect(()=>{api.get<Category[]>("/categories").then(r=>setItems(Array.isArray(r.data)?r.data:[])).catch(()=>setError("Não foi possível carregar as categorias.")).finally(()=>setLoading(false))},[]);
 return <><PageHeader title="Categorias" description="Organize seus gastos e entradas."/><div className="panel">{error&&<div className="alert error">{error}</div>}{loading?<Loading/>:items.length?<div className="table-wrap"><table><thead><tr><th>Nome</th><th>Tipo</th><th>Status</th></tr></thead><tbody>{items.map(c=><tr key={c.id}><td>{c.name}</td><td>{c.type}</td><td>{c.isActive?"Ativa":"Inativa"}</td></tr>)}</tbody></table></div>:<EmptyState title="Nenhuma categoria" description="Cadastre categorias para entender seus gastos."/>}</div></>;
}
