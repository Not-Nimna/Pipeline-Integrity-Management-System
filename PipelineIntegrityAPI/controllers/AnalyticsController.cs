using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PipelineIntegrityAPI.Data;
using PipelineIntegrityAPI.Dtos;
using PipelineIntegrityAPI.Models;

namespace PipelineIntegrityAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AnalyticsController(AppDbContext db) => _db = db;

        // POST: api/analytics/recompute/{segmentId}
        [HttpPost("recompute/{segmentId:guid}")]
        public async Task<ActionResult<RecomputeRiskResultDto>> Recompute(Guid segmentId)
        {
            var segment = await _db.Segments
                .Include(s => s.Inspections)
                .Include(s => s.RiskScore)
                .FirstOrDefaultAsync(s => s.SegmentId == segmentId);

            if (segment is null) return NotFound("Segment not found.");

            var latest = segment.Inspections
                .OrderByDescending(i => i.InspectionDate)
                .FirstOrDefault();

            // If no inspections yet, set risk to 0/Low
            var score = latest is null ? 0 : ComputeRisk(latest.Method, latest.MaxDepthPct);
            var severity = ScoreToSeverity(score);

            if (segment.RiskScore is null)
            {
                segment.RiskScore = new RiskScore
                {
                    SegmentId = segment.SegmentId,
                    Score = score,
                    Severity = severity,
                    UpdatedAt = DateTime.UtcNow
                };
                _db.RiskScores.Add(segment.RiskScore);
            }
            else
            {
                segment.RiskScore.Score = score;
                segment.RiskScore.Severity = severity;
                segment.RiskScore.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();

            return Ok(new RecomputeRiskResultDto(
                segment.SegmentId,
                score,
                severity,
                segment.RiskScore!.UpdatedAt
            ));
        }

        // GET: api/analytics/summary
        [HttpGet("summary")]
        public async Task<ActionResult<DashboardSummaryDto>> Summary()
        {
            var pipelineCount = await _db.Pipelines.CountAsync();
            var segmentCount = await _db.Segments.CountAsync();
            var inspectionCount = await _db.Inspections.CountAsync();

            var highRiskCount = await _db.RiskScores.CountAsync(r => r.Severity == "High");

            var top = await _db.Segments
                .AsNoTracking()
                .Include(s => s.RiskScore)
                .Where(s => s.RiskScore != null)
                .OrderByDescending(s => s.RiskScore!.Score)
                .Take(10)
                .Select(s => new SegmentRiskRowDto(
                    s.SegmentId,
                    s.PipelineId,
                    s.Name,
                    s.RiskScore!.Score,
                    s.RiskScore!.Severity
                ))
                .ToListAsync();

            return Ok(new DashboardSummaryDto(
                pipelineCount,
                segmentCount,
                inspectionCount,
                highRiskCount,
                top
            ));
        }

        private static int ComputeRisk(string method, int maxDepthPct)
        {
            // Simple, explainable MVP scoring
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
