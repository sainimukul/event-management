import { useQuery } from "@tanstack/react-query";
import { registrationsApi } from "../../../api/registrationsApi";

export function useRegistrations(eventId: string | undefined) {
  return useQuery({
    queryKey: ["events", eventId, "registrations"],
    queryFn: () => registrationsApi.list(eventId!),
    enabled: !!eventId,
  });
}
