import axios from "axios";

export interface ApiErrorInfo {
  message: string;
  fieldErrors?: Record<string, string[]>;
  status?: number;
}

export function extractApiError(err: unknown): ApiErrorInfo {
  if (axios.isAxiosError(err) && err.response) {
    const { status, data } = err.response;
    if (data && typeof data === "object") {
      // ValidationProblemDetails
      if ("errors" in data && data.errors && typeof data.errors === "object") {
        return {
          status,
          message: (data as { title?: string }).title ?? "Validation failed.",
          fieldErrors: data.errors as Record<string, string[]>,
        };
      }
      // ErrorResponse
      if ("message" in data) {
        return { status, message: String((data as { message: unknown }).message) };
      }
    }
    return { status, message: err.message };
  }
  return { message: err instanceof Error ? err.message : "Unknown error" };
}
