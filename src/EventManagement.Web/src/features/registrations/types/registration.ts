export interface Registration {
  id: string;
  eventId: string;
  userId: string;
  userName: string;
  registeredAt: string;
}

export interface RegisterUserPayload {
  userId: string;
  userName: string;
}
