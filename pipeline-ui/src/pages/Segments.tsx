import { useEffect, useMemo, useState } from "react";
import { api } from "../api/client";
import type { PipelineListDto, SegmentCreateDto, SegmentListDto } from "../api/types";

export default function Segments() {
  const [pipelines, setPipelines] = useState<PipelineListDto[]>([]);
  const [pipelineId, setPipelineId] = useState<string>("");

  const [rows, setRows] = useState<SegmentListDto[]>([]);
  const [err, setErr] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  const canCreate = useMemo(() => pipelineId.length > 0, [pipelineId]);

  const [form, setForm] = useState<Omit<SegmentCreateDto, "pipelineId">>({
    name: "",
    startLat: 51.0447,
    startLng: -114.0719,
    endLat: 51.05,
    endLng: -114.06,
    lengthKm: 1.2,
  });

  async function loadPipelines() {
    const data = await api.listPipelines();
    setPipelines(data);
  }

  async function refreshSegments(selectedPipelineId?: string) {
    setLoading(true);
    setErr(null);
    try {
      const data = await api.listSegments(selectedPipelineId || pipelineId || undefined);
      setRows(data);
    } catch (e) {
      setErr(e instanceof Error ? e.message : "Failed to load segments");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadPipelines().catch((e) => setErr(e instanceof Error ? e.message : "Failed to load pipelines"));
  }, []);

  useEffect(() => {
    refreshSegments().catch(() => {});
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [pipelineId]);

  async function onCreate(e: React.FormEvent) {
    e.preventDefault();
    if (!pipelineId) return;

    setErr(null);
    try {
      await api.createSegment({
        pipelineId,
        name: form.name.trim(),
        startLat: Number(form.startLat),
        startLng: Number(form.startLng),
        endLat: Number(form.endLat),
        endLng: Number(form.endLng),
        lengthKm: Number(form.lengthKm),
      });
      setForm((f) => ({ ...f, name: "" }));
      await refreshSegments(pipelineId);
    } catch (e) {
      setErr(e instanceof Error ? e.message : "Create failed");
    }
  }

  async function onDelete(id: string) {
    if (!confirm("Delete segment? This will delete inspections & risk score.")) return;
    setErr(null);
    try {
      await api.deleteSegment(id);
      await refreshSegments(pipelineId || undefined);
    } catch (e) {
      setErr(e instanceof Error ? e.message : "Delete failed");
    }
  }

  async function onRecompute(segmentId: string) {
    setErr(null);
    try {
      await api.recomputeRisk(segmentId);
      await refreshSegments(pipelineId || undefined);
    } catch (e) {
      setErr(e instanceof Error ? e.message : "Recompute failed");
    }
  }

  return (
    <div className="page">
      <h1>Segments</h1>

      <div className="card">
        <div className="row">
          <div>
            <label className="inlineLabel">
              Filter by pipeline:
              <select value={pipelineId} onChange={(e) => setPipelineId(e.target.value)}>
                <option value="">All</option>
                {pipelines.map((p) => (
                  <option key={p.pipelineId} value={p.pipelineId}>
                    {p.name}
                  </option>
                ))}
              </select>
            </label>
          </div>
          <button className="btn" onClick={() => refreshSegments(pipelineId || undefined)}>
            Refresh
          </button>
        </div>
      </div>

      <div className="grid2">
        <div className="card">
          <h2>Create Segment</h2>
          {!canCreate && <div className="muted">Select a pipeline to add a segment.</div>}
          <form onSubmit={onCreate} className="form">
            <label>
              Pipeline
              <select value={pipelineId} onChange={(e) => setPipelineId(e.target.value)} required>
                <option value="" disabled>
                  Select…
                </option>
                {pipelines.map((p) => (
                  <option key={p.pipelineId} value={p.pipelineId}>
                    {p.name}
                  </option>
                ))}
              </select>
            </label>

            <label>
              Segment Name
              <input value={form.name} onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))} required />
            </label>

            <div className="grid2tight">
              <label>
                Start Lat
                <input type="number" step="0.000001" value={form.startLat} onChange={(e) => setForm((f) => ({ ...f, startLat: Number(e.target.value) }))} />
              </label>
              <label>
                Start Lng
                <input type="number" step="0.000001" value={form.startLng} onChange={(e) => setForm((f) => ({ ...f, startLng: Number(e.target.value) }))} />
              </label>
              <label>
                End Lat
                <input type="number" step="0.000001" value={form.endLat} onChange={(e) => setForm((f) => ({ ...f, endLat: Number(e.target.value) }))} />
              </label>
              <label>
                End Lng
                <input type="number" step="0.000001" value={form.endLng} onChange={(e) => setForm((f) => ({ ...f, endLng: Number(e.target.value) }))} />
              </label>
            </div>

            <label>
              Length (km)
              <input type="number" step="0.001" value={form.lengthKm} onChange={(e) => setForm((f) => ({ ...f, lengthKm: Number(e.target.value) }))} />
            </label>

            <button className="btn" type="submit" disabled={!canCreate}>
              Create
            </button>
            {err && <div className="error">{err}</div>}
          </form>
        </div>

        <div className="card">
          <h2>Segments</h2>
          {loading ? (
            <div className="muted">Loading…</div>
          ) : (
            <table className="table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Severity</th>
                  <th>Score</th>
                  <th>Latest Insp.</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {rows.length === 0 ? (
                  <tr>
                    <td colSpan={5} className="muted">
                      No segments yet.
                    </td>
                  </tr>
                ) : (
                  rows.map((s) => (
                    <tr key={s.segmentId}>
                      <td>{s.name}</td>
                      <td>{s.severity ?? "—"}</td>
                      <td>{s.riskScore ?? "—"}</td>
                      <td>{s.latestInspectionDate ?? "—"}</td>
                      <td className="right">
                        <button className="btn" onClick={() => onRecompute(s.segmentId)}>
                          Recompute
                        </button>{" "}
                        <button className="btn danger" onClick={() => onDelete(s.segmentId)}>
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
