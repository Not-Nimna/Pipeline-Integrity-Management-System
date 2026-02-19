
namespace PipelineIntegrityAPI.Dtos
{
    public record RecomputeRiskResultDto(
        Guid SegmentId,
        int Score,
        string Severity,
        DateTime UpdatedAt
    );

    public record DashboardSummaryDto(
        int PipelineCount,
        int SegmentCount,
        int InspectionCount,
        int HighRiskSegmentCount,
        List<SegmentRiskRowDto> TopRiskSegments
    );

    public record SegmentRiskRowDto(
        Guid SegmentId,
        Guid PipelineId,
        string SegmentName,
        int Score,
        string Severity
    );
}
