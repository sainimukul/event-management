import { apiClient } from "./client";
import type { Registration, RegisterUserPayload } from "../features/registrations/types/registration";

/**
 * Per-resource API module for registrations. All routes are nested under
 * `/events/{eventId}/registrations/...` to mirror the parent-child resource shape on the backend.
 */
export const registrationsApi = {
  /** GET `/events/{eventId}/registrations` — list registrations for one event. */
  list: async (eventId: string): Promise<Registration[]> => {
    const { data } = await apiClient.get<Registration[]>(`/events/${eventId}/registrations`);
    return data;
  },
  /**
   * POST `/events/{eventId}/registrations` — register a user. The server enforces past-date,
   * capacity, and duplicate-user rules and responds with 422 if any fails.
   */
  register: async (eventId: string, payload: RegisterUserPayload): Promise<Registration> => {
    const { data } = await apiClient.post<Registration>(`/events/${eventId}/registrations`, payload);
    return data;
  },
  /** DELETE `/events/{eventId}/registrations/{registrationId}` — remove a registration. */
  unregister: async (eventId: string, registrationId: string): Promise<void> => {
    await apiClient.delete(`/events/${eventId}/registrations/${registrationId}`);
  },
};
