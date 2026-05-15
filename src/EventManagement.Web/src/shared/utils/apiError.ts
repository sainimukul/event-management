import axios from "axios";

/**
 * Normalised error info parsed out of an axios error. The two backend error shapes (400
 * `ValidationProblemDetails` for DataAnnotations failures and the custom `ErrorResponse`
 * for 404/422/500) are flattened into a single structure so call sites don't have to
 * branch on which one came back.
 */
export interface ApiErrorInfo {
  /** Top-level message suitable for a toast or banner. */
  message: string;
  /** Field-level errors keyed by property name, only set for 400 ValidationProblemDetails. */
  fieldErrors?: Record<string, string[]>;
  /** HTTP status code, when available. */
  status?: number;
}

/**
 * Best-effort parse of an unknown error value into {@link ApiErrorInfo}. Recognises the
 * two backend error shapes, falls back to the axios message if neither matches, and to a
 * generic "Unknown error" for non-axios values.
 */
export function extractApiError(err: unknown): ApiErrorInfo {
  if (axios.isAxiosError(err) && err.response) {
    const { status, data } = err.response;
    if (data && typeof data === "object") {
      // ValidationProblemDetails — emitted by ASP.NET Core's [ApiController] when
      // DataAnnotations fail. `errors` is a dict of field name → string[] messages.
      if ("errors" in data && data.errors && typeof data.errors === "object") {
        return {
          status,
          message: (data as { title?: string }).title ?? "Validation failed.",
          fieldErrors: data.errors as Record<string, string[]>,
        };
      }
      // ErrorResponse — emitted by ExceptionHandlingMiddleware for 404/422/500.
      if ("message" in data) {
        return { status, message: String((data as { message: unknown }).message) };
      }
    }
    return { status, message: err.message };
  }
  return { message: err instanceof Error ? err.message : "Unknown error" };
}
