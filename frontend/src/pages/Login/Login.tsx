import { FormEvent, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../../contexts/AuthContext";
import { getErrorMessage } from "../../services/api";

export default function Login() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  async function submit(e: FormEvent) {
    e.preventDefault(); setError(""); setLoading(true);
    try { await login(email, password); navigate("/"); }
    catch (err) { setError(getErrorMessage(err, "E-mail ou senha inválidos.")); }
    finally { setLoading(false); }
  }

  return <div className="auth-page">
    <div className="auth-panel">
      <div className="auth-logo"><div className="brand-mark large">F</div><h1>FinanceControl</h1></div>
      <p className="auth-subtitle">Controle seu dinheiro de forma simples.</p>
      <form onSubmit={submit} className="form">
        <label>E-mail<input className="input" type="email" value={email} onChange={e => setEmail(e.target.value)} placeholder="voce@email.com" required /></label>
        <label>Senha<input className="input" type="password" value={password} onChange={e => setPassword(e.target.value)} placeholder="••••••••" required /></label>
        {error && <div className="alert error">{error}</div>}
        <button className="primary-btn" disabled={loading}>{loading ? "Entrando..." : "Entrar"}</button>
      </form>
      <p className="auth-footer">Ainda não possui conta? <Link to="/register">Criar conta</Link></p>
    </div>
  </div>;
}
