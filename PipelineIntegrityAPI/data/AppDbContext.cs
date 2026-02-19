using Microsoft.EntityFrameworkCore;
using PipelineIntegrityAPI.Models;

namespace PipelineIntegrityAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Pipeline> Pipelines => Set<Pipeline>();
        public DbSet<Segment> Segments => Set<Segment>();
        public DbSet<Inspection> Inspections => Set<Inspection>();
        public DbSet<RiskScore> RiskScores => Set<RiskScore>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Pipeline 1..* Segments
            modelBuilder.Entity<Segment>()
                .HasOne(s => s.Pipeline)
                .WithMany(p => p.Segments)
                .HasForeignKey(s => s.PipelineId)
                .OnDelete(DeleteBehavior.Cascade);

            // Segment 1..* Inspections
            modelBuilder.Entity<Inspection>()
                .HasOne(i => i.Segment)
                .WithMany(s => s.Inspections)
                .HasForeignKey(i => i.SegmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Segment 1..1 RiskScore
            modelBuilder.Entity<RiskScore>()
                .HasOne(r => r.Segment)
                .WithOne(s => s.RiskScore)
                .HasForeignKey<RiskScore>(r => r.SegmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RiskScore>()
                .HasIndex(r => r.SegmentId)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}
