import { Routes, Route, Link } from "react-router-dom";

function Placeholder({ title }: { title: string }) {
  return <div className="card">{title} — coming soon</div>;
}

export default function App() {
  return (
    <div className="app-shell">
      <header className="page-header">
        <Link to="/" style={{ fontWeight: 700, fontSize: "1.25rem", color: "var(--color-text)" }}>
          Event Manager
        </Link>
      </header>
      <Routes>
        <Route path="/" element={<Placeholder title="Event List" />} />
        <Route path="/events/new" element={<Placeholder title="Create Event" />} />
        <Route path="/events/:id" element={<Placeholder title="Event Detail" />} />
      </Routes>
    </div>
  );
}
