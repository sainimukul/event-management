import { useQuery } from "@tanstack/react-query";
import { eventsApi } from "../../../api/eventsApi";

/**
 * Loads the full event list. Cached under the `["events"]` query key — every mutation that
 * changes any event (create, update, register, unregister) invalidates this key so the list
 * reflects the change.
 */
export function useEvents() {
  return useQuery({ queryKey: ["events"], queryFn: eventsApi.list });
}
