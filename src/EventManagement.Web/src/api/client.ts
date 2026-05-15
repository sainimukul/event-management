import axios from "axios";

/**
 * Shared axios instance used by every per-resource API module. The base URL is read from
 * `VITE_API_BASE_URL` so the same build can target different environments — the dev `.env`
 * points it at the local backend on `:5050` (which is also what the backend's CORS policy
 * whitelists in `:5173`). The path version (`/api/v1`) lives here, not on each call site,
 * so a future bump to `/api/v2` is a single edit.
 */
export const apiClient = axios.create({
  baseURL: (import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5050") + "/api/v1",
  headers: { "Content-Type": "application/json" },
});
