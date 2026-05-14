import { useMutation, useQueryClient } from "@tanstack/react-query";
import { registrationsApi } from "../../../api/registrationsApi";
import type { RegisterUserPayload } from "../types/registration";

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
