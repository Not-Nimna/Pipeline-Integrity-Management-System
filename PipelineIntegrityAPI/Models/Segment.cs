using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PipelineIntegrityAPI.Models
{
    public class Segment
    {
        [Key]
        public Guid SegmentId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PipelineId { get; set; }

        [ForeignKey(nameof(PipelineId))]
        public Pipeline? Pipeline { get; set; }

        [Required, MaxLength(120)]
        public string Name { get; set; } = string.Empty;

        // Store coordinates (simple MVP)
        [Column(TypeName = "decimal(9,6)")]
        public decimal StartLat { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal StartLng { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal EndLat { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal EndLng { get; set; }

        [Column(TypeName = "decimal(8,3)")]
        public decimal LengthKm { get; set; }

        // Navigation
        public List<Inspection> Inspections { get; set; } = new();
        public RiskScore? RiskScore { get; set; }
    }
}
