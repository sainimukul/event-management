import { useQuery } from "@tanstack/react-query";
import { eventsApi } from "../../../api/eventsApi";

export function useEvents() {
  return useQuery({ queryKey: ["events"], queryFn: eventsApi.list });
}
