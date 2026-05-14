import { useNavigate, Link } from "react-router-dom";
import toast from "react-hot-toast";
import { EventForm } from "../components/EventForm";
import { useCreateEvent } from "../hooks/useCreateEvent";

export function CreateEventPage() {
  const navigate = useNavigate();
  const mutation = useCreateEvent();

  return (
    <section>
      <div className="page-header">
        <h1>New event</h1>
        <Link to="/" className="button button--ghost">Back</Link>
      </div>
      <EventForm
        mode="create"
        onSubmit={async (payload) => {
          const created = await mutation.mutateAsync(payload);
          toast.success("Event created");
          navigate(`/events/${created.id}`);
        }}
        onCancel={() => navigate("/")}
      />
    </section>
  );
}
