namespace PipelineIntegrityAPI.Dtos
{
    public record InspectionCreateDto(
        Guid SegmentId,
        DateOnly InspectionDate,
        string Method,
        int MaxDepthPct,
        string? Notes
    );

    public record InspectionListDto(
        Guid InspectionId,
        Guid SegmentId,
        DateOnly InspectionDate,
        string Method,
        int MaxDepthPct,
        string? Notes
    );
}
