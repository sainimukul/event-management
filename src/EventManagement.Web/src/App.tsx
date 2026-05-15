import { Routes, Route, Link } from "react-router-dom";
import { EventListPage } from "./features/events/pages/EventListPage";
import { CreateEventPage } from "./features/events/pages/CreateEventPage";
import { EventDetailPage } from "./features/events/pages/EventDetailPage";

/**
 * Top-level app shell and route table. Three routes only — list, create, detail — kept
 * flat on purpose. The provider setup (router, TanStack Query, toaster) lives in `main.tsx`
 * so this component stays focused on layout.
 */
export default function App() {
  return (
    <div className="app-shell">
      <header className="page-header">
        <Link to="/" style={{ fontWeight: 700, fontSize: "1.25rem", color: "var(--color-text)" }}>
          Event Manager
        </Link>
      </header>
      <Routes>
        <Route path="/" element={<EventListPage />} />
        <Route path="/events/new" element={<CreateEventPage />} />
        <Route path="/events/:id" element={<EventDetailPage />} />
      </Routes>
    </div>
  );
}
