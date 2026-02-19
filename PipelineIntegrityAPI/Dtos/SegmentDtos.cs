namespace PipelineIntegrityAPI.Dtos
{
    public record SegmentCreateDto(
        Guid PipelineId,
        string Name,
        decimal StartLat, decimal StartLng,
        decimal EndLat, decimal EndLng,
        decimal LengthKm
    );

    public record SegmentUpdateDto(
        string Name,
        decimal StartLat, decimal StartLng,
        decimal EndLat, decimal EndLng,
        decimal LengthKm
    );

    public record SegmentListDto(
        Guid SegmentId,
        Guid PipelineId,
        string Name,
        decimal StartLat, decimal StartLng,
        decimal EndLat, decimal EndLng,
        decimal LengthKm,
        int? RiskScore,
        string? Severity,
        DateOnly? LatestInspectionDate
    );
}
