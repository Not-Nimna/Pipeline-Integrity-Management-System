import { useEffect, useState } from "react";
import { api } from "../api/client";
import type { DashboardSummaryDto } from "../api/types";

export default function Dashboard() {
  const [data, setData] = useState<DashboardSummaryDto | null>(null);
  const [err, setErr] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let alive = true;

    api
      .getSummary()
      .then((d) => alive && setData(d))
      .catch((e) => alive && setErr(e.message ?? "Failed to load"))
      .finally(() => alive && setLoading(false));
    return () => {
      alive = false;
    };
  }, []);

  return (
    <div className="page">
      <h1>Dashboard</h1>

      {loading && <div className="card">Loading…</div>}
      {err && <div className="card error">{err}</div>}

      {data && (
        <>
          <div className="grid4">
            <div className="card">
              <div className="kpiLabel">Pipelines</div>
              <div className="kpiValue">{data.pipelineCount}</div>
            </div>
            <div className="card">
              <div className="kpiLabel">Segments</div>
              <div className="kpiValue">{data.segmentCount}</div>
            </div>
            <div className="card">
              <div className="kpiLabel">Inspections</div>
              <div className="kpiValue">{data.inspectionCount}</div>
            </div>
            <div className="card">
              <div className="kpiLabel">High Risk</div>
              <div className="kpiValue">{data.highRiskSegmentCount}</div>
            </div>
          </div>

          <div className="card">
            <h2>Top Risk Segments</h2>
            <table className="table">
              <thead>
                <tr>
                  <th>Segment</th>
                  <th>Severity</th>
                  <th>Score</th>
                </tr>
              </thead>
              <tbody>
                {data.topRiskSegments.length === 0 ? (
                  <tr>
                    <td colSpan={3} className="muted">
                      No risk scores yet. Add inspections and recompute.
                    </td>
                  </tr>
                ) : (
                  data.topRiskSegments.map((r) => (
                    <tr key={r.segmentId}>
                      <td>{r.segmentName}</td>
                      <td>{r.severity}</td>
                      <td>{r.score}</td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </>
      )}
    </div>
  );
}
