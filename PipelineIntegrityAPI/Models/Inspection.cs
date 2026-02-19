using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PipelineIntegrityAPI.Models
{
    public class Inspection
    {
        [Key]
        public Guid InspectionId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid SegmentId { get; set; }

        [ForeignKey(nameof(SegmentId))]
        public Segment? Segment { get; set; }

        [Required]
        public DateOnly InspectionDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

        [Required, MaxLength(50)]
        public string Method { get; set; } = "ILI"; // ILI, CPCM, Visual...

        [Range(0, 100)]
        public int MaxDepthPct { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}
