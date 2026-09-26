import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from "react";
import api from "../services/api";
import type { AuthResponse } from "../types";

type AuthContextValue = {
  token: string | null;
  login: (email: string, password: string) => Promise<void>;
  register: (name: string, email: string, password: string) => Promise<void>;
  logout: () => void;
};

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | null>(() => localStorage.getItem("financecontrol_token"));

  const logout = () => {
    localStorage.removeItem("financecontrol_token");
    setToken(null);
  };

  useEffect(() => {
    const handler = () => logout();
    window.addEventListener("financecontrol:logout", handler);
    return () => window.removeEventListener("financecontrol:logout", handler);
  }, []);

  async function login(email: string, password: string) {
    const response = await api.post<AuthResponse>("/auth/login", { email, password });
    const nextToken = response.data.accessToken;
    if (!nextToken) throw new Error("A API não retornou o token de acesso.");
    localStorage.setItem("financecontrol_token", nextToken);
    setToken(nextToken);
  }

  async function register(name: string, email: string, password: string) {
    await api.post("/auth/register", { name, email, password });
  }

  const value = useMemo(() => ({ token, login, register, logout }), [token]);
  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const value = useContext(AuthContext);
  if (!value) throw new Error("useAuth deve ser usado dentro de AuthProvider.");
  return value;
}
