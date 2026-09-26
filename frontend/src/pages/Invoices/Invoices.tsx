import { useCallback, useEffect, useState } from "react";
import api, { getErrorMessage } from "../../services/api";
import { onFinanceDataChanged } from "../../services/financeEvents";
import type { Account, Invoice } from "../../types";
import PageHeader from "../../components/PageHeader";
import Loading from "../../components/Loading";
import EmptyState from "../../components/EmptyState";

const money = (value: number) => new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" }).format(value);
const date = (value: string) => new Date(`${value.slice(0, 10)}T00:00:00`).toLocaleDateString("pt-BR");
const invoiceStatus: Record<string, string> = { "1": "Aberta", "2": "Fechada", Open: "Aberta", Closed: "Fechada" };

export default function Invoices() {
  const [items, setItems] = useState<Invoice[]>([]);
  const [cards, setCards] = useState<Account[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const load = useCallback(async () => {
    setError("");
    try {
      const { data: cardList } = await api.get<Account[]>("/credit-cards");
      const cardItems = Array.isArray(cardList) ? cardList : [];
      const responses = await Promise.all(cardItems.map((card) => api.get<Invoice[]>(`/invoices?creditCardAccountId=${encodeURIComponent(card.id)}`)));
      setCards(cardItems);
      setItems(responses.flatMap((response) => Array.isArray(response.data) ? response.data : []));
    } catch (err) { setError(getErrorMessage(err)); }
    finally { setLoading(false); }
  }, []);
  useEffect(() => { void load(); return onFinanceDataChanged(() => { void load(); }); }, [load]);

  return <>
    <PageHeader title="Faturas" description="Consulte período, fechamento, vencimento e total das faturas." />
    {error && <div className="alert error" role="alert">{error}</div>}
    <div className="panel">{loading ? <Loading /> : items.length ? <div className="table-wrap"><table><thead><tr><th>Cartão</th><th>Período</th><th>Fechamento</th><th>Vencimento</th><th>Status</th><th>Total</th></tr></thead><tbody>
      {items.map(invoice => <tr key={invoice.id}><td>{cards.find((card) => card.id === invoice.creditCardAccountId)?.name ?? "Cartão"}</td><td>{String(invoice.periodMonth).padStart(2, "0")}/{invoice.periodYear}</td><td>{date(invoice.closingDate)}</td><td>{date(invoice.dueDate)}</td><td><span className="badge">{invoiceStatus[String(invoice.status)] ?? invoice.status}</span></td><td className="money">{money(invoice.totalAmount)}</td></tr>)}
    </tbody></table></div> : <EmptyState title="Nenhuma fatura" description={cards.length ? "As compras feitas no cartão criarão as faturas automaticamente." : "Cadastre um cartão para começar a acompanhar faturas."} />}</div>
  </>;
}
