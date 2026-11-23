import { createContext, useContext, useEffect, useState, type ReactNode } from "react";
import api from "../services/api";

type Session = { userId: string; name: string };
type AuthContextValue = {
  session: Session | null;
  loading: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (name: string, email: string, password: string, inviteCode?: string) => Promise<void>;
  logout: () => Promise<void>;
};

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<Session | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    localStorage.removeItem("financecontrol_token");
    const abort = new AbortController();
    const clearSession = () => setSession(null);
    window.addEventListener("financecontrol:logout", clearSession);
    api.get<Session>("/auth/me", { signal: abort.signal })
      .then(({ data }) => setSession(data))
      .catch(() => undefined)
      .finally(() => { if (!abort.signal.aborted) setLoading(false); });
    return () => {
      abort.abort();
      window.removeEventListener("financecontrol:logout", clearSession);
    };
  }, []);

  async function login(email: string, password: string) {
    await api.post("/auth/login", { email, password });
    const { data } = await api.get<Session>("/auth/me");
    setSession(data);
  }

  async function register(name: string, email: string, password: string, inviteCode?: string) {
    await api.post("/auth/register", { name, email, password, inviteCode });
  }

  async function logout() {
    await api.post("/auth/logout");
    setSession(null);
  }

  return <AuthContext.Provider value={{ session, loading, login, register, logout }}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const value = useContext(AuthContext);
  if (!value) throw new Error("useAuth deve ser usado dentro de AuthProvider.");
  return value;
}
