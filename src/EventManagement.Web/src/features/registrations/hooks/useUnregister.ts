import { useMutation, useQueryClient } from "@tanstack/react-query";
import { registrationsApi } from "../../../api/registrationsApi";

/**
 * Mutation that removes a single registration. Invalidates the same three keys as
 * `useRegister` so every view of this event's counts stays consistent.
 */
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
