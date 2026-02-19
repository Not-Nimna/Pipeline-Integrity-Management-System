import type { DashboardSummaryDto, InspectionCreateDto, InspectionListDto, PipelineCreateDto, PipelineListDto, SegmentCreateDto, SegmentListDto } from "./types";

const BASE = import.meta.env.VITE_API_BASE_URL?.replace(/\/$/, "") ?? "";

async function http<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(`${BASE}${path}`, {
    headers: { "Content-Type": "application/json", ...(init?.headers ?? {}) },
    ...init,
  });

  if (!res.ok) {
    const msg = await res.text().catch(() => "");
    throw new Error(msg || `Request failed (${res.status})`);
  }

  // Some endpoints return empty (204)
  if (res.status === 204) return undefined as T;
  return (await res.json()) as T;
}

export const api = {
  // Dashboard
  getSummary: () => http<DashboardSummaryDto>("/api/analytics/summary"),

  // Pipelines
  listPipelines: () => http<PipelineListDto[]>("/api/pipelines"),
  createPipeline: (dto: PipelineCreateDto) => http<PipelineListDto>("/api/pipelines", { method: "POST", body: JSON.stringify(dto) }),
  deletePipeline: (id: string) => http<void>(`/api/pipelines/${id}`, { method: "DELETE" }),

  // Segments
  listSegments: (pipelineId?: string) => http<SegmentListDto[]>(pipelineId ? `/api/segments?pipelineId=${encodeURIComponent(pipelineId)}` : "/api/segments"),
  createSegment: (dto: SegmentCreateDto) => http<{ segmentId: string }>("/api/segments", { method: "POST", body: JSON.stringify(dto) }),
  deleteSegment: (id: string) => http<void>(`/api/segments/${id}`, { method: "DELETE" }),

  // Inspections
  listInspections: (segmentId: string) => http<InspectionListDto[]>(`/api/inspections?segmentId=${encodeURIComponent(segmentId)}`),
  createInspection: (dto: InspectionCreateDto) => http<{ inspectionId: string }>("/api/inspections", { method: "POST", body: JSON.stringify(dto) }),
  deleteInspection: (id: string) => http<void>(`/api/inspections/${id}`, { method: "DELETE" }),

  // Analytics / Risk
  recomputeRisk: (segmentId: string) => http<void>(`/api/analytics/recompute/${segmentId}`, { method: "POST" }),
};
