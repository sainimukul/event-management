import { useMutation, useQueryClient } from "@tanstack/react-query";
import { registrationsApi } from "../../../api/registrationsApi";
import type { RegisterUserPayload } from "../types/registration";

/**
 * Mutation that registers a user for an event. On success invalidates all three keys that
 * touch this event's state:
 *   - `["events", eventId, "registrations"]` — the registration list itself
 *   - `["events", eventId]` — the detail's `currentRegistrations` counter
 *   - `["events"]`         — the list page's count column
 *
 * Keep this triple invalidation in sync when adding new mutations: every key that includes
 * data the mutation touched must be invalidated.
 */
export function useRegister(eventId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (payload: RegisterUserPayload) => registrationsApi.register(eventId, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["events", eventId] });
      qc.invalidateQueries({ queryKey: ["events", eventId, "registrations"] });
      qc.invalidateQueries({ queryKey: ["events"] });
    },
  });
}
