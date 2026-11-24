import { useState } from "react";
import { getErrorMessage } from "../services/api";
import { NavLink, Outlet } from "react-router-dom";
import { useAuth } from "../contexts/AuthContext";

const links = [
  { to: "/dashboard", label: "Dashboard", icon: "◫" },
  { to: "/accounts", label: "Contas", icon: "▣" },
  { to: "/transactions", label: "Lançamentos", icon: "↕" },
  { to: "/categories", label: "Categorias", icon: "◈" },
  { to: "/cards", label: "Cartões", icon: "▭" },
  { to: "/invoices", label: "Faturas", icon: "▤" },
  { to: "/budgets", label: "Orçamento", icon: "◷" },
  { to: "/debts", label: "Dívidas", icon: "◫" },
];

export default function AppLayout() {
  const { logout, session } = useAuth();
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);
  async function signOut() {
    setBusy(true);
    setError("");
    try { await logout(); }
    catch (err) { setError(getErrorMessage(err, "Não foi possível encerrar a sessão.")); }
    finally { setBusy(false); }
  }
  return <div className="app-shell"><aside className="sidebar">
    <div className="brand"><div className="brand-mark">F</div><div><strong>FinanceControl</strong><small>Controle financeiro</small></div></div>
    <nav aria-label="Navegação principal">{links.map(link => <NavLink key={link.to} to={link.to}><span aria-hidden="true">{link.icon}</span>{link.label}</NavLink>)}</nav>
    <div className="sidebar-bottom"><button className="logout-btn" onClick={signOut} disabled={busy}>↪ <span>Sair</span></button></div>
  </aside><div className="main-shell"><header className="topbar"><span className="topbar-title">Minhas finanças</span><div className="user-dot" aria-label="Perfil">{session?.name?.charAt(0).toUpperCase()}</div></header><main className="content">{error && <div className="alert error" role="alert">{error}</div>}<Outlet/></main></div></div>;
}
