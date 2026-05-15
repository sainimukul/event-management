import { apiClient } from "./client";
import type { Registration, RegisterUserPayload } from "../features/registrations/types/registration";

export const registrationsApi = {
  list: async (eventId: string): Promise<Registration[]> => {
    const { data } = await apiClient.get<Registration[]>(`/events/${eventId}/registrations`);
    return data;
  },
  register: async (eventId: string, payload: RegisterUserPayload): Promise<Registration> => {
    const { data } = await apiClient.post<Registration>(`/events/${eventId}/registrations`, payload);
    return data;
  },
  unregister: async (eventId: string, registrationId: string): Promise<void> => {
    await apiClient.delete(`/events/${eventId}/registrations/${registrationId}`);
  },
};
