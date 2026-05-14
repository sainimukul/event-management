export interface EventSummary {
  id: string;
  title: string;
  date: string;
  maxCapacity: number;
  currentRegistrations: number;
  createdAt: string;
}

export interface EventDetail extends EventSummary {
  description?: string | null;
  updatedAt: string;
}

export interface EventFormValues {
  title: string;
  description: string;
  date: string;
  maxCapacity: number;
}
