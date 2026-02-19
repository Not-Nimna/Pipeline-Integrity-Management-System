import { useEffect, useState } from "react";
import { api } from "../api/client";
import type { PipelineCreateDto, PipelineListDto } from "../api/types";

export default function Pipelines() {
  const [rows, setRows] = useState<PipelineListDto[]>([]);
  const [err, setErr] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  const [form, setForm] = useState<PipelineCreateDto>({
    name: "",
    operator: "",
    status: "Active",
  });

  async function refresh() {
    setLoading(true);
    setErr(null);
    try {
      const data = await api.listPipelines();
      setRows(data);
    } catch (e) {
      setErr(e instanceof Error ? e.message : "Failed to load");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    refresh();
  }, []);

  async function onCreate(e: React.FormEvent) {
    e.preventDefault();
    setErr(null);
    try {
      await api.createPipeline({
        name: form.name.trim(),
        operator: form.operator?.trim() || null,
        status: form.status?.trim() || "Active",
      });
      setForm({ name: "", operator: "", status: "Active" });
      await refresh();
    } catch (e) {
      setErr(e instanceof Error ? e.message : "Create failed");
    }
  }

  async function onDelete(id: string) {
    if (!confirm("Delete pipeline? This will delete segments too.")) return;
    setErr(null);
    try {
      await api.deletePipeline(id);
      await refresh();
    } catch (e) {
      setErr(e instanceof Error ? e.message : "Delete failed");
    }
  }

  return (
    <div className="page">
      <h1>Pipelines</h1>

      <div className="grid2">
        <div className="card">
          <h2>Create Pipeline</h2>
          <form onSubmit={onCreate} className="form">
            <label>
              Name
              <input value={form.name} onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))} required />
            </label>
            <label>
              Operator
              <input value={form.operator ?? ""} onChange={(e) => setForm((f) => ({ ...f, operator: e.target.value }))} />
            </label>
            <label>
              Status
              <select value={form.status ?? "Active"} onChange={(e) => setForm((f) => ({ ...f, status: e.target.value }))}>
                <option>Active</option>
                <option>Inactive</option>
                <option>Maintenance</option>
              </select>
            </label>

            <button className="btn" type="submit">
              Create
            </button>
            {err && <div className="error">{err}</div>}
          </form>
        </div>

        <div className="card">
          <h2>All Pipelines</h2>
          {loading ? (
            <div className="muted">Loading…</div>
          ) : (
            <table className="table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Operator</th>
                  <th>Status</th>
                  <th>Segments</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {rows.length === 0 ? (
                  <tr>
                    <td colSpan={5} className="muted">
                      No pipelines yet.
                    </td>
                  </tr>
                ) : (
                  rows.map((p) => (
                    <tr key={p.pipelineId}>
                      <td>{p.name}</td>
                      <td>{p.operator ?? "—"}</td>
                      <td>{p.status}</td>
                      <td>{p.segmentCount}</td>
                      <td className="right">
                        <button className="btn danger" onClick={() => onDelete(p.pipelineId)}>
                          Delete
                        </button>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          )}
        </div>
      </div>
    </div>
  );
}
