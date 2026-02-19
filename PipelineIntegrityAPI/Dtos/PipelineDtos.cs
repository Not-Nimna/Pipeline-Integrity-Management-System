namespace PipelineIntegrityAPI.Dtos
{
    public record PipelineCreateDto(string Name, string? Operator, string? Status);
    public record PipelineUpdateDto(string Name, string? Operator, string? Status);

    public record PipelineListDto(
        Guid PipelineId,
        string Name,
        string? Operator,
        string Status,
        DateTime CreatedAt,
        int SegmentCount
    );

    public record PipelineDetailDto(
        Guid PipelineId,
        string Name,
        string? Operator,
        string Status,
        DateTime CreatedAt,
        List<SegmentMapDto> Segments
    );

    // Small segment shape used inside pipeline detail
    public record SegmentMapDto(
        Guid SegmentId,
        Guid PipelineId,
        string Name,
        decimal StartLat, decimal StartLng,
        decimal EndLat, decimal EndLng,
        decimal LengthKm,
        int? RiskScore,
        string? Severity
    );
}
