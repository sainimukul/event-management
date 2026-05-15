import { useMutation, useQueryClient } from "@tanstack/react-query";
import { eventsApi, type CreateEventPayload } from "../../../api/eventsApi";

export function useCreateEvent() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateEventPayload) => eventsApi.create(payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["events"] }),
  });
}
