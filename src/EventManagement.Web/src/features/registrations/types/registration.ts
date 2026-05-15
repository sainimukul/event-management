/**
 * Registration entity as returned by the backend. Mirrors `RegistrationDto` server-side.
 * Immutable from the frontend's perspective — unregistering deletes the row rather than
 * mutating it.
 */
export interface Registration {
  /** Stable registration identifier (GUID string). */
  id: string;
  /** The event this registration belongs to. */
  eventId: string;
  /** External identifier of the registering user. */
  userId: string;
  /** Display name supplied at registration time. */
  userName: string;
  /** When the registration was accepted, ISO 8601 UTC. */
  registeredAt: string;
}

/**
 * Request body for `POST /events/{eventId}/registrations`. Authentication is out of scope
 * for the exercise, so the user identity is carried in the body.
 */
export interface RegisterUserPayload {
  userId: string;
  userName: string;
}
