using System.ComponentModel.DataAnnotations;

namespace PipelineIntegrityAPI.Models
{
    public class Pipeline
    {
        [Key]
        public Guid PipelineId { get; set; } = Guid.NewGuid();

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Operator { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Active";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public List<Segment> Segments { get; set; } = new();
    }
}
