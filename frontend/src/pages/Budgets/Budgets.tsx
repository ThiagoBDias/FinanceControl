import { PageHeader } from "../../components/PageHeader";
import EmptyState from "../../components/EmptyState";
export default function Budgets(){return <><PageHeader title="Orçamento" description="Compare o que você planejou com o que já gastou."/><div className="panel"><EmptyState title="Orçamentos" description="A tela está preparada para mostrar limite, gasto, restante e excedente por categoria." /></div></>;}
