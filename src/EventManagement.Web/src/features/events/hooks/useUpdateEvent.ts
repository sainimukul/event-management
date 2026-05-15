import { useMutation, useQueryClient } from "@tanstack/react-query";
import { eventsApi, type CreateEventPayload } from "../../../api/eventsApi";

/**
 * Mutation to update an existing event. Invalidates both the list (`["events"]`) and the
 * specific detail (`["events", id]`) cache so the change is reflected wherever the event
 * is shown.
 */
export function useUpdateEvent(id: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateEventPayload) => eventsApi.update(id, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["events"] });
      qc.invalidateQueries({ queryKey: ["events", id] });
    },
  });
}
