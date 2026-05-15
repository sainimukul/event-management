import { useMutation, useQueryClient } from "@tanstack/react-query";
import { eventsApi, type CreateEventPayload } from "../../../api/eventsApi";

/**
 * Mutation to create a new event. On success invalidates the `["events"]` list cache so
 * the new event appears immediately. Does not touch detail or registration keys — they
 * don't exist for an event that didn't exist before this call.
 */
export function useCreateEvent() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateEventPayload) => eventsApi.create(payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["events"] }),
  });
}
