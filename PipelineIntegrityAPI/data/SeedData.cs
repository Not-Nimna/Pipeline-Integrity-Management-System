// Data/SeedData.cs
using Microsoft.EntityFrameworkCore;
using PipelineIntegrityAPI.Models;

namespace PipelineIntegrityAPI.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(AppDbContext db)
        {
            // Ensure DB is ready (safe even if already migrated)
            await db.Database.MigrateAsync();

            // Idempotent: don’t reseed if data already exists
            if (await db.Pipelines.AnyAsync())
                return;

            // ---------- Pipelines ----------
            var p1 = new Pipeline
            {
                Name = "Calgary Northline",
                Operator = "Demo Operator",
                Status = "Active"
            };

            var p2 = new Pipeline
            {
                Name = "Bow River Connector",
                Operator = "Demo Operator",
                Status = "Active"
            };

            db.Pipelines.AddRange(p1, p2);
            await db.SaveChangesAsync();

            // ---------- Segments (Calgary-ish coordinates) ----------
            // Rough coordinates around NW/NE Calgary so the Leaflet map looks real.
            var s1 = new Segment
            {
                PipelineId = p1.PipelineId,
                Name = "Crowfoot → Tuscany",
                StartLat = 51.123500m, StartLng = -114.207200m,
                EndLat = 51.127800m, EndLng = -114.248500m,
                LengthKm = 6.200m
            };

            var s2 = new Segment
            {
                PipelineId = p1.PipelineId,
                Name = "Tuscany → Nolan Hill",
                StartLat = 51.127800m, StartLng = -114.248500m,
                EndLat = 51.160900m, EndLng = -114.245100m,
                LengthKm = 4.800m
            };

            var s3 = new Segment
            {
                PipelineId = p1.PipelineId,
                Name = "Nolan Hill → Beacon Hill",
                StartLat = 51.160900m, StartLng = -114.245100m,
                EndLat = 51.162900m, EndLng = -114.156600m,
                LengthKm = 7.400m
            };

            var s4 = new Segment
            {
                PipelineId = p2.PipelineId,
                Name = "Saddletowne → Martindale",
                StartLat = 51.129700m, StartLng = -113.965600m,
                EndLat = 51.121900m, EndLng = -113.954100m,
                LengthKm = 2.300m
            };

            var s5 = new Segment
            {
                PipelineId = p2.PipelineId,
                Name = "Martindale → Whitehorn",
                StartLat = 51.121900m, StartLng = -113.954100m,
                EndLat = 51.084900m, EndLng = -113.996600m,
                LengthKm = 8.100m
            };

            var s6 = new Segment
            {
                PipelineId = p2.PipelineId,
                Name = "Whitehorn → Deerfoot Hub",
                StartLat = 51.084900m, StartLng = -113.996600m,
                EndLat = 51.074800m, EndLng = -114.008700m,
                LengthKm = 1.900m
            };

            db.Segments.AddRange(s1, s2, s3, s4, s5, s6);
            await db.SaveChangesAsync();

            // ---------- Inspections ----------
            // Add a few per segment so analytics + recompute looks good.
            var inspections = new List<Inspection>
            {
                new Inspection { SegmentId = s1.SegmentId, InspectionDate = new DateOnly(2025, 10, 12), Method = "ILI",    MaxDepthPct = 28, Notes = "Minor metal loss indications." },
                new Inspection { SegmentId = s1.SegmentId, InspectionDate = new DateOnly(2026,  1, 15), Method = "CPCM",   MaxDepthPct = 34, Notes = "Coating review; moderate indications." },

                new Inspection { SegmentId = s2.SegmentId, InspectionDate = new DateOnly(2025,  9,  5), Method = "Visual", MaxDepthPct = 12, Notes = "No critical issues observed." },
                new Inspection { SegmentId = s2.SegmentId, InspectionDate = new DateOnly(2026,  1, 20), Method = "ILI",    MaxDepthPct = 41, Notes = "In-line inspection flagged areas to monitor." },

                new Inspection { SegmentId = s3.SegmentId, InspectionDate = new DateOnly(2025,  8, 19), Method = "ILI",    MaxDepthPct = 77, Notes = "High depth feature; prioritize mitigation." },

                new Inspection { SegmentId = s4.SegmentId, InspectionDate = new DateOnly(2025, 11,  2), Method = "CPCM",   MaxDepthPct = 22, Notes = "Coating anomalies detected." },
                new Inspection { SegmentId = s4.SegmentId, InspectionDate = new DateOnly(2026,  1, 28), Method = "ILI",    MaxDepthPct = 36, Notes = "Moderate corrosion growth trend." },

                new Inspection { SegmentId = s5.SegmentId, InspectionDate = new DateOnly(2025,  7, 10), Method = "Visual", MaxDepthPct = 18, Notes = "Routine survey." },
                new Inspection { SegmentId = s5.SegmentId, InspectionDate = new DateOnly(2026,  2,  1), Method = "ILI",    MaxDepthPct = 63, Notes = "Multiple features clustered; watchlist." },

                new Inspection { SegmentId = s6.SegmentId, InspectionDate = new DateOnly(2025, 12, 14), Method = "CPCM",   MaxDepthPct = 30, Notes = "Surface indications; follow-up recommended." }
            };

            db.Inspections.AddRange(inspections);
            await db.SaveChangesAsync();

            // ---------- RiskScores (based on latest inspection per segment) ----------
            // This mirrors the logic in AnalyticsController so dashboard works immediately.
            foreach (var seg in new[] { s1, s2, s3, s4, s5, s6 })
            {
                var latest = inspections
                    .Where(i => i.SegmentId == seg.SegmentId)
                    .OrderByDescending(i => i.InspectionDate)
                    .FirstOrDefault();

                var score = latest is null ? 0 : ComputeRisk(latest.Method, latest.MaxDepthPct);
                var severity = ScoreToSeverity(score);

                db.RiskScores.Add(new RiskScore
                {
                    SegmentId = seg.SegmentId,
                    Score = score,
                    Severity = severity,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            await db.SaveChangesAsync();
        }

        private static int ComputeRisk(string method, int maxDepthPct)
        {
            var weight = method.Trim().ToUpperInvariant() switch
            {
                "ILI" => 10,
                "CPCM" => 5,
                "VISUAL" => 0,
                _ => 2
            };

            var score = maxDepthPct + weight;
            if (score < 0) score = 0;
            if (score > 100) score = 100;
            return score;
        }

        private static string ScoreToSeverity(int score)
        {
            if (score < 35) return "Low";
            if (score < 70) return "Med";
            return "High";
        }
    }
}
