import { useMutation, useQueryClient } from "@tanstack/react-query";
import { registrationsApi } from "../../../api/registrationsApi";

export function useUnregister(eventId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (registrationId: string) => registrationsApi.unregister(eventId, registrationId),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["events", eventId] });
      qc.invalidateQueries({ queryKey: ["events", eventId, "registrations"] });
      qc.invalidateQueries({ queryKey: ["events"] });
    },
  });
}
