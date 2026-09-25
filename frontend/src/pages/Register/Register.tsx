import { FormEvent, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../../contexts/AuthContext";
import { getErrorMessage } from "../../services/api";

export default function Register() {
  const { register } = useAuth(); const navigate = useNavigate();
  const [name,setName]=useState(""); const [email,setEmail]=useState(""); const [password,setPassword]=useState(""); const [confirm,setConfirm]=useState("");
  const [error,setError]=useState(""); const [loading,setLoading]=useState(false);
  async function submit(e: FormEvent) {
    e.preventDefault(); setError("");
    if (password !== confirm) { setError("As senhas não conferem."); return; }
    setLoading(true);
    try { await register(name,email,password); navigate("/login"); }
    catch(err) { setError(getErrorMessage(err)); } finally { setLoading(false); }
  }
  return <div className="auth-page"><div className="auth-panel">
    <div className="auth-logo"><div className="brand-mark large">F</div><h1>Criar conta</h1></div>
    <p className="auth-subtitle">Comece a organizar suas finanças.</p>
    <form onSubmit={submit} className="form">
      <label>Nome<input className="input" value={name} onChange={e=>setName(e.target.value)} required /></label>
      <label>E-mail<input className="input" type="email" value={email} onChange={e=>setEmail(e.target.value)} required /></label>
      <label>Senha<input className="input" type="password" value={password} onChange={e=>setPassword(e.target.value)} required /></label>
      <label>Confirmar senha<input className="input" type="password" value={confirm} onChange={e=>setConfirm(e.target.value)} required /></label>
      {error && <div className="alert error">{error}</div>}
      <button className="primary-btn" disabled={loading}>{loading ? "Criando..." : "Criar conta"}</button>
    </form>
    <p className="auth-footer">Já possui conta? <Link to="/login">Entrar</Link></p>
  </div></div>;
}
