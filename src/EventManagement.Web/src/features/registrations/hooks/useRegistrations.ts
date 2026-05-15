import { useQuery } from "@tanstack/react-query";
import { registrationsApi } from "../../../api/registrationsApi";

/**
 * Loads the registrations for a single event. Cached under `["events", eventId, "registrations"]`
 * — register/unregister mutations invalidate this exact key. Gated by `enabled: !!eventId`
 * so the hook can be called before the route param resolves.
 */
export function useRegistrations(eventId: string | undefined) {
  return useQuery({
    queryKey: ["events", eventId, "registrations"],
    queryFn: () => registrationsApi.list(eventId!),
    enabled: !!eventId,
  });
}
