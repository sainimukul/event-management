import { useMutation, useQueryClient } from "@tanstack/react-query";
import { eventsApi, type CreateEventPayload } from "../../../api/eventsApi";

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
