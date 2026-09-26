import { useCallback, useEffect, useState, type FormEvent } from "react";
import api, { getErrorMessage } from "../../services/api";
import { notifyFinanceDataChanged, onFinanceDataChanged } from "../../services/financeEvents";
import type { Category } from "../../types";
import PageHeader from "../../components/PageHeader";
import Loading from "../../components/Loading";
import EmptyState from "../../components/EmptyState";
import Modal from "../../components/Modal";

const categoryType: Record<string, string> = { "1": "Entrada", "2": "Despesa" };

export default function Categories() {
  const [items, setItems] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [notice, setNotice] = useState("");
  const [createOpen, setCreateOpen] = useState(false);
  const [editing, setEditing] = useState<Category | null>(null);
  const [busy, setBusy] = useState(false);

  const load = useCallback(async () => {
    setError("");
    try { const { data } = await api.get<Category[]>("/categories"); setItems(Array.isArray(data) ? data : []); }
    catch (err) { setError(getErrorMessage(err)); }
    finally { setLoading(false); }
  }, []);
  useEffect(() => { void load(); return onFinanceDataChanged(() => { void load(); }); }, [load]);

  async function save(event: FormEvent<HTMLFormElement>) {
    event.preventDefault(); setBusy(true); setError("");
    const form = new FormData(event.currentTarget);
    try {
      await api.post("/categories", { name: String(form.get("name")).trim(), type: Number(form.get("type")) });
      setCreateOpen(false); setNotice("Categoria criada com sucesso."); notifyFinanceDataChanged();
    } catch (err) { setError(getErrorMessage(err)); }
    finally { setBusy(false); }
  }
  async function rename(event: FormEvent<HTMLFormElement>) {
    event.preventDefault(); if (!editing) return; setBusy(true); setError("");
    const form = new FormData(event.currentTarget);
    try {
      await api.put(`/categories/${editing.id}`, { name: String(form.get("name")).trim() });
      setEditing(null); setNotice("Categoria atualizada."); notifyFinanceDataChanged();
    } catch (err) { setError(getErrorMessage(err)); }
    finally { setBusy(false); }
  }
  async function deactivate(category: Category) {
    if (!window.confirm(`Desativar a categoria “${category.name}”?`)) return;
    setError("");
    try { await api.post(`/categories/${category.id}/deactivate`); setNotice("Categoria desativada."); notifyFinanceDataChanged(); }
    catch (err) { setError(getErrorMessage(err)); }
  }

  return <>
    <PageHeader title="Categorias" description="Organize seus gastos e entradas." action={<button className="primary-btn" onClick={() => { setError(""); setCreateOpen(true); }}>+ Nova categoria</button>} />
    {notice && <div className="alert success" role="status">{notice}<button className="alert-dismiss" onClick={() => setNotice("")}>×</button></div>}
    {error && <div className="alert error" role="alert">{error}</div>}
    <div className="panel">{loading ? <Loading /> : items.length ? <div className="table-wrap"><table><thead><tr><th>Nome</th><th>Tipo</th><th>Status</th><th>Ações</th></tr></thead><tbody>{items.map(category =>
      <tr key={category.id}><td>{category.name}</td><td>{categoryType[String(category.type)] ?? category.type}</td><td><span className="badge">{category.isActive ? "Ativa" : "Inativa"}</span></td><td><div className="row-actions"><button className="secondary-btn" onClick={() => { setError(""); setEditing(category); }}>Editar</button>{category.isActive && <button className="danger-btn" onClick={() => void deactivate(category)}>Desativar</button>}</div></td></tr>)}</tbody></table></div> : <EmptyState title="Nenhuma categoria" description="Cadastre uma categoria para classificar suas entradas ou despesas." action={<button className="primary-btn" onClick={() => setCreateOpen(true)}>+ Nova categoria</button>} />}</div>
    {createOpen && <Modal title="Nova categoria" onClose={() => setCreateOpen(false)} onSubmit={save} busy={busy} error={error}>
      <label>Nome<input className="input" name="name" required maxLength={100} autoFocus /></label>
      <label>Tipo<select className="input" name="type" defaultValue="2"><option value="1">Entrada</option><option value="2">Despesa</option></select></label>
    </Modal>}
    {editing && <Modal title="Editar categoria" onClose={() => setEditing(null)} onSubmit={rename} busy={busy} error={error}>
      <label>Nome<input className="input" name="name" required maxLength={100} defaultValue={editing.name} autoFocus /></label>
      <p className="form-hint">O tipo não pode ser alterado após a criação.</p>
    </Modal>}
  </>;
}
