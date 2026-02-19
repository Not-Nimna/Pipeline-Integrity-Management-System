export type PipelineListDto = {
  pipelineId: string;
  name: string;
  operator?: string | null;
  status: string;
  createdAt: string;
  segmentCount: number;
};

export type PipelineCreateDto = {
  name: string;
  operator?: string | null;
  status?: string | null;
};

export type SegmentListDto = {
  segmentId: string;
  pipelineId: string;
  name: string;
  startLat: number;
  startLng: number;
  endLat: number;
  endLng: number;
  lengthKm: number;
  riskScore?: number | null;
  severity?: string | null;
  latestInspectionDate?: string | null;
};

export type SegmentCreateDto = {
  pipelineId: string;
  name: string;
  startLat: number;
  startLng: number;
  endLat: number;
  endLng: number;
  lengthKm: number;
};

export type InspectionListDto = {
  inspectionId: string;
  segmentId: string;
  inspectionDate: string; // DateOnly serialized as "YYYY-MM-DD"
  method: string;
  maxDepthPct: number;
  notes?: string | null;
};

export type InspectionCreateDto = {
  segmentId: string;
  inspectionDate: string; // "YYYY-MM-DD"
  method: string;
  maxDepthPct: number;
  notes?: string | null;
};

export type SegmentRiskRowDto = {
  segmentId: string;
  pipelineId: string;
  segmentName: string;
  score: number;
  severity: string;
};

export type DashboardSummaryDto = {
  pipelineCount: number;
  segmentCount: number;
  inspectionCount: number;
  highRiskSegmentCount: number;
  topRiskSegments: SegmentRiskRowDto[];
};
