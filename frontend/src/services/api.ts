import axios, { type InternalAxiosRequestConfig } from "axios";

const configuredApiUrl = import.meta.env.VITE_API_URL?.replace(/\/+$/, "");
const apiBaseUrl = configuredApiUrl
  ? configuredApiUrl.endsWith("/api") ? configuredApiUrl : `${configuredApiUrl}/api`
  : "/api";

type FinancialConfig = InternalAxiosRequestConfig & { retryStorageKey?: string };
const api = axios.create({
  baseURL: apiBaseUrl,
  withCredentials: true,
  timeout: 90_000,
  headers: { "Content-Type": "application/json", "X-FinanceControl-Client": "browser" },
});

api.interceptors.request.use(async (config: FinancialConfig) => {
  if (config.method !== "post" || config.url?.startsWith("/auth/")) return config;

  // Keep the request UUID through ambiguous network failures and tab reloads.
  // Only a hash of the submitted values is stored in the browser.
  const payload = new TextEncoder().encode(JSON.stringify([config.url, config.data ?? null]));
  const digest = new Uint8Array(await crypto.subtle.digest("SHA-256", payload));
  const hash = Array.from(digest, (byte) => byte.toString(16).padStart(2, "0")).join("");
  const storageKey = `financecontrol:request:${hash}`;
  const requestId = sessionStorage.getItem(storageKey) ?? crypto.randomUUID();
  sessionStorage.setItem(storageKey, requestId);
  config.retryStorageKey = storageKey;
  config.headers.set("Idempotency-Key", requestId);
  return config;
});

function finishRequest(config?: FinancialConfig) {
  if (config?.retryStorageKey) sessionStorage.removeItem(config.retryStorageKey);
}

api.interceptors.response.use(
  (response) => { finishRequest(response.config); return response; },
  (error) => {
    const status = error?.response?.status;
    if (status >= 400 && status < 500) finishRequest(error.config);
    if (status === 401 && !error.config?.url?.startsWith("/auth/")) {
      window.dispatchEvent(new Event("financecontrol:logout"));
    }
    return Promise.reject(error);
  }
);

export function getErrorMessage(error: unknown, fallback = "Não foi possível concluir a operação.") {
  if (axios.isAxiosError(error)) {
    if (!error.response) return "Falha de conexão. Confira a rede e tente novamente.";
    if (error.response.status === 429) return "Muitas tentativas. Aguarde um minuto e tente novamente.";
    const data = error.response.data as { message?: string; detail?: string; errors?: Record<string, string[]>; validation?: { errors?: Array<{ message?: string }> } } | undefined;
    if (data?.message) return data.message;
    if (data?.validation?.errors?.length) return data.validation.errors.map((item) => item.message).filter(Boolean).join(" ");
    if (data?.errors) {
      const first = Object.values(data.errors).flat()[0];
      if (first) return first;
    }
    return data?.detail ?? fallback;
  }
  return error instanceof Error ? error.message : fallback;
}

export default api;
