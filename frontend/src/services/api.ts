import axios from "axios";

const configuredApiUrl = import.meta.env.VITE_API_URL?.replace(/\/+$/, "");
const apiBaseUrl = configuredApiUrl
  ? configuredApiUrl.endsWith("/api") ? configuredApiUrl : `${configuredApiUrl}/api`
  : "/api";

const api = axios.create({
  baseURL: apiBaseUrl,
  headers: { "Content-Type": "application/json" },
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem("financecontrol_token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error?.response?.status === 401) {
      localStorage.removeItem("financecontrol_token");
      window.dispatchEvent(new Event("financecontrol:logout"));
    }
    return Promise.reject(error);
  }
);

export function getErrorMessage(error: unknown, fallback = "Não foi possível concluir a operação.") {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data as { message?: string; detail?: string; title?: string; errors?: Record<string, string[]>; validation?: { errors?: Array<{ message?: string }> } } | undefined;
    if (data?.message) return data.message;
    if (data?.detail) return data.detail;
    if (data?.validation?.errors?.length) return data.validation.errors.map((item) => item.message).filter(Boolean).join(" ");
    if (data?.errors) {
      const first = Object.values(data.errors).flat()[0];
      if (first) return first;
    }
  }
  if (error instanceof Error) return error.message;
  return fallback;
}

export default api;
