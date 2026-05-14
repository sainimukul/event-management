import { Link } from "react-router-dom";
import { useEvents } from "../hooks/useEvents";
import { EventCard } from "../components/EventCard";
import { Spinner } from "../../../shared/components/Spinner";
import { EmptyState } from "../../../shared/components/EmptyState";
import { ErrorBanner } from "../../../shared/components/ErrorBanner";

export function EventListPage() {
  const { data, isLoading, isError, refetch } = useEvents();

  return (
    <section>
      <div className="page-header">
        <h1>Events</h1>
        <Link to="/events/new" className="button button--primary">New event</Link>
      </div>

      {isLoading && <Spinner label="Loading events..." />}
      {isError && <ErrorBanner message="Failed to load events." onRetry={() => refetch()} />}
      {data && data.length === 0 && (
        <EmptyState
          title="No events yet."
          action={<Link to="/events/new" className="button button--primary">Create your first event</Link>}
        />
      )}
      {data && data.length > 0 && (
        <div>{data.map((ev) => <EventCard key={ev.id} event={ev} />)}</div>
      )}
    </section>
  );
}
