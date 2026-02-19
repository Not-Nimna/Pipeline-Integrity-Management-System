import { NavLink, Route, Routes } from "react-router-dom";
import Dashboard from "./pages/Dashboard";
import Pipelines from "./pages/Pipelines";
import Segments from "./pages/Segments";
import Inspections from "./pages/Inspections";
import "./styles.css";

export default function App() {
  return (
    <div className="app">
      <header className="topbar">
        <div className="brand">Pipeline Integrity MVP</div>
        <nav className="nav">
          <NavLink to="/" end className={({ isActive }) => (isActive ? "active" : "")}>
            Dashboard
          </NavLink>
          <NavLink to="/pipelines" className={({ isActive }) => (isActive ? "active" : "")}>
            Pipelines
          </NavLink>
          <NavLink to="/segments" className={({ isActive }) => (isActive ? "active" : "")}>
            Segments
          </NavLink>
          <NavLink to="/inspections" className={({ isActive }) => (isActive ? "active" : "")}>
            Inspections
          </NavLink>
        </nav>
      </header>

      <main className="container">
        <Routes>
          <Route path="/" element={<Dashboard />} />
          <Route path="/pipelines" element={<Pipelines />} />
          <Route path="/segments" element={<Segments />} />
          <Route path="/inspections" element={<Inspections />} />
        </Routes>
      </main>
    </div>
  );
}
