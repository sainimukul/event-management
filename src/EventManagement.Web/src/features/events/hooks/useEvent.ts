import { useQuery } from "@tanstack/react-query";
import { eventsApi } from "../../../api/eventsApi";

/**
 * Loads a single event's full detail. Cached under `["events", id]`. The `enabled` guard
 * keeps the query idle until the id is available (e.g. while a route param resolves),
 * which lets us call the hook unconditionally from the component.
 */
export function useEvent(id: string | undefined) {
  return useQuery({
    queryKey: ["events", id],
    queryFn: () => eventsApi.get(id!),
    enabled: !!id,
  });
}
