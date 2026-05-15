import { useQuery } from "@tanstack/react-query";
import { eventsApi } from "../../../api/eventsApi";

export function useEvent(id: string | undefined) {
  return useQuery({
    queryKey: ["events", id],
    queryFn: () => eventsApi.get(id!),
    enabled: !!id,
  });
}
