using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PipelineIntegrityAPI.Models
{
    public class RiskScore
    {
        [Key]
        public Guid RiskScoreId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid SegmentId { get; set; }

        [ForeignKey(nameof(SegmentId))]
        public Segment? Segment { get; set; }

        [Range(0, 100)]
        public int Score { get; set; }

        [Required, MaxLength(10)]
        public string Severity { get; set; } = "Low"; // Low/Med/High

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
