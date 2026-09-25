import { NavLink, Outlet } from "react-router-dom";
import { useAuth } from "../contexts/AuthContext";

const links = [
  { to: "/", label: "Dashboard", icon: "⌂", end: true },
  { to: "/accounts", label: "Contas", icon: "▣" },
  { to: "/transactions", label: "Lançamentos", icon: "↕" },
  { to: "/categories", label: "Categorias", icon: "◈" },
  { to: "/cards", label: "Cartões", icon: "▭" },
  { to: "/invoices", label: "Faturas", icon: "▤" },
  { to: "/budgets", label: "Orçamento", icon: "◷" },
];

export default function AppLayout() {
  const { logout } = useAuth();
  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="brand"><div className="brand-mark">F</div><div><strong>FinanceControl</strong><small>Controle financeiro</small></div></div>
        <nav>{links.map((link) => <NavLink key={link.to} to={link.to} end={link.end}><span>{link.icon}</span>{link.label}</NavLink>)}</nav>
        <div className="sidebar-bottom"><button className="logout-btn" onClick={logout}>↪ <span>Sair</span></button></div>
      </aside>
      <div className="main-shell">
        <header className="topbar"><div><span className="topbar-title">Minhas finanças</span></div><div className="user-dot">T</div></header>
        <main className="content"><Outlet /></main>
      </div>
    </div>
  );
}
