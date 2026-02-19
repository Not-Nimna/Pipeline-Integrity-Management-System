import { useEffect, useState } from "react";
import { api } from "../api/client";
import type { InspectionCreateDto, InspectionListDto, PipelineListDto, SegmentListDto } from "../api/types";

export default function Inspections() {
  const [pipelines, setPipelines] = useState<PipelineListDto[]>([]);
  const [segments, setSegments] = useState<SegmentListDto[]>([]);

  const [pipelineId, setPipelineId] = useState<string>("");
  const [segmentId, setSegmentId] = useState<string>("");

  const [rows, setRows] = useState<InspectionListDto[]>([]);
  const [err, setErr] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const [form, setForm] = useState<Omit<InspectionCreateDto, "segmentId">>({
    inspectionDate: new Date().toISOString().slice(0, 10), // YYYY-MM-DD
    method: "ILI",
    maxDepthPct: 10,
    notes: "",
  });

  useEffect(() => {
    api
      .listPipelines()
      .then(setPipelines)
      .catch((e) => setErr(e instanceof Error ? e.message : "Failed to load pipelines"));
  }, []);

  useEffect(() => {
    if (!pipelineId) {
      setSegments([]);
      setSegmentId("");
      return;
    }
    api
      .listSegments(pipelineId)
      .then((s) => {
        setSegments(s);
        setSegmentId(""); // user selects
      })
      .catch((e) => setErr(e.message ?? "Failed to load segments"));
  }, [pipelineId]);

  async function loadInspections(segId: string) {
    setLoading(true);
    setErr(null);
    try {
      const data = await api.listInspections(segId);
      setRows(data);
    } catch (e) {
      setErr(e instanceof Error ? e.message : "Failed to load inspections");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    if (!segmentId) {
      setRows([]);
      return;
    }
    loadInspections(segmentId).catch(() => {});
  }, [segmentId]);

  async function onCreate(e: React.FormEvent) {
    e.preventDefault();
    if (!segmentId) return;
    setErr(null);
    try {
      await api.createInspection({
        segmentId,
        inspectionDate: form.inspectionDate,
        method: form.method.trim(),
        maxDepthPct: Number(form.maxDepthPct),
        notes: form.notes?.trim() || null,
      });
      setForm((f) => ({ ...f, notes: "" }));
      await api.recomputeRisk(segmentId); // nice UX: auto update risk
      await loadInspections(segmentId);
    } catch (e) {
      setErr(e instanceof Error ? e.message : "Create failed");
    }
  }

  async function onDelete(id: string) {
    if (!confirm("Delete inspection?")) return;
    setErr(null);
    try {
      await api.deleteInspection(id);
      if (segmentId) {
        await api.recomputeRisk(segmentId);
        await loadInspections(segmentId);
      }
    } catch (e) {
      setErr(e instanceof Error ? e.message : "Delete failed");
    }
  }

  return (
    <div className="page">
      <h1>Inspections</h1>

      <div className="card">
        <div className="grid3">
          <label>
            Pipeline
            <select value={pipelineId} onChange={(e) => setPipelineId(e.target.value)}>
              <option value="">Select…</option>
              {pipelines.map((p) => (
                <option key={p.pipelineId} value={p.pipelineId}>
                  {p.name}
                </option>
              ))}
            </select>
          </label>

          <label>
            Segment
            <select value={segmentId} onChange={(e) => setSegmentId(e.target.value)} disabled={!pipelineId}>
              <option value="">Select…</option>
              {segments.map((s) => (
                <option key={s.segmentId} value={s.segmentId}>
                  {s.name}
                </option>
              ))}
            </select>
          </label>

          <div className="row end">
            <button className="btn" onClick={() => segmentId && loadInspections(segmentId)} disabled={!segmentId}>
              Refresh
            </button>
          </div>
        </div>
      </div>

      <div className="grid2">
        <div className="card">
          <h2>Add Inspection</h2>
          {!segmentId && <div className="muted">Select a segment to add an inspection.</div>}
          <form onSubmit={onCreate} className="form">
            <label>
              Date
              <input type="date" value={form.inspectionDate} onChange={(e) => setForm((f) => ({ ...f, inspectionDate: e.target.value }))} disabled={!segmentId} />
            </label>

            <label>
              Method
              <select value={form.method} onChange={(e) => setForm((f) => ({ ...f, method: e.target.value }))} disabled={!segmentId}>
                <option value="ILI">ILI</option>
                <option value="CPCM">CPCM</option>
                <option value="Visual">Visual</option>
              </select>
            </label>

            <label>
              Max Depth (%)
              <input type="number" min={0} max={100} value={form.maxDepthPct} onChange={(e) => setForm((f) => ({ ...f, maxDepthPct: Number(e.target.value) }))} disabled={!segmentId} />
            </label>

            <label>
              Notes
              <input value={form.notes ?? ""} onChange={(e) => setForm((f) => ({ ...f, notes: e.target.value }))} disabled={!segmentId} />
            </label>

            <button className="btn" type="submit" disabled={!segmentId}>
              Add
            </button>

            {err && <div className="error">{err}</div>}
          </form>
        </div>

        <div className="card">
          <h2>Inspections</h2>
          {loading ? (
            <div className="muted">Loading…</div>
          ) : (
            <table className="table">
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Method</th>
                  <th>Max Depth</th>
                  <th>Notes</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {rows.length === 0 ? (
                  <tr>
                    <td colSpan={5} className="muted">
                      No inspections yet.
                    </td>
                  </tr>
                ) : (
                  rows.map((i) => (
                    <tr key={i.inspectionId}>
                      <td>{i.inspectionDate}</td>
                      <td>{i.method}</td>
                      <td>{i.maxDepthPct}%</td>
                      <td>{i.notes ?? "—"}</td>
                      <td className="right">
                        <button className="btn danger" onClick={() => onDelete(i.inspectionId)}>
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
