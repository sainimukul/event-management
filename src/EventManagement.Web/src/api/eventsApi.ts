import { apiClient } from "./client";
import type { EventSummary, EventDetail } from "../features/events/types/event";

/**
 * Request body for `POST /api/v1/events` and `PUT /api/v1/events/{id}`. The two endpoints
 * accept the same shape today; if they diverge in the future they should grow distinct types.
 */
export interface CreateEventPayload {
  /** Event title; required, server trims and enforces 1–200 chars. */
  title: string;
  /** Optional long-form description; max 2000 chars. Pass `null` to clear. */
  description?: string | null;
  /** ISO 8601 timestamp; the backend stores UTC. */
  date: string;
  /** Maximum number of registrations. Must be ≥ 1. */
  maxCapacity: number;
}

/**
 * Per-resource API module for events. Each method maps 1:1 to a backend endpoint and
 * returns parsed response data — error handling lives in `extractApiError` at the call site
 * (typically wrapped in a TanStack Query hook).
 */
export const eventsApi = {
  /** GET `/events` — list all events with their registration counts. */
  list: async (): Promise<EventSummary[]> => {
    const { data } = await apiClient.get<EventSummary[]>("/events");
    return data;
  },
  /** GET `/events/{id}` — fetch the full detail of a single event. */
  get: async (id: string): Promise<EventDetail> => {
    const { data } = await apiClient.get<EventDetail>(`/events/${id}`);
    return data;
  },
  /** POST `/events` — create a new event. Returns the created summary. */
  create: async (payload: CreateEventPayload): Promise<EventSummary> => {
    const { data } = await apiClient.post<EventSummary>("/events", payload);
    return data;
  },
  /** PUT `/events/{id}` — replace an event's fields in place. */
  update: async (id: string, payload: CreateEventPayload): Promise<EventSummary> => {
    const { data } = await apiClient.put<EventSummary>(`/events/${id}`, payload);
    return data;
  },
};
