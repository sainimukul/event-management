import { apiClient } from "./client";
import type { EventSummary, EventDetail } from "../features/events/types/event";

export interface CreateEventPayload {
  title: string;
  description?: string | null;
  date: string;
  maxCapacity: number;
}

export const eventsApi = {
  list: async (): Promise<EventSummary[]> => {
    const { data } = await apiClient.get<EventSummary[]>("/api/events");
    return data;
  },
  get: async (id: string): Promise<EventDetail> => {
    const { data } = await apiClient.get<EventDetail>(`/api/events/${id}`);
    return data;
  },
  create: async (payload: CreateEventPayload): Promise<EventSummary> => {
    const { data } = await apiClient.post<EventSummary>("/api/events", payload);
    return data;
  },
  update: async (id: string, payload: CreateEventPayload): Promise<EventSummary> => {
    const { data } = await apiClient.put<EventSummary>(`/api/events/${id}`, payload);
    return data;
  },
};
