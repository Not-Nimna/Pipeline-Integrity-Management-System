using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PipelineIntegrityAPI.Data;
using PipelineIntegrityAPI.Dtos;
using PipelineIntegrityAPI.Models;

namespace PipelineIntegrityAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SegmentsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public SegmentsController(AppDbContext db) => _db = db;

        // GET: api/segments?pipelineId=...
        [HttpGet]
        public async Task<ActionResult<List<SegmentListDto>>> GetAll([FromQuery] Guid? pipelineId)
        {
            var query = _db.Segments
                .AsNoTracking()
                .Include(s => s.RiskScore)
                .Include(s => s.Inspections)
                .AsQueryable();

            if (pipelineId.HasValue)
                query = query.Where(s => s.PipelineId == pipelineId.Value);

            var segments = await query
                .OrderBy(s => s.Name)
                .Select(s => new SegmentListDto(
                    s.SegmentId,
                    s.PipelineId,
                    s.Name,
                    s.StartLat, s.StartLng,
                    s.EndLat, s.EndLng,
                    s.LengthKm,
                    s.RiskScore != null ? s.RiskScore.Score : null,
                    s.RiskScore != null ? s.RiskScore.Severity : null,
                    s.Inspections
                        .OrderByDescending(i => i.InspectionDate)
                        .Select(i => (DateOnly?)i.InspectionDate)
                        .FirstOrDefault()
                ))
                .ToListAsync();

            return Ok(segments);
        }

        // POST: api/segments
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SegmentCreateDto dto)
        {
            // Ensure pipeline exists
            var exists = await _db.Pipelines.AnyAsync(p => p.PipelineId == dto.PipelineId);
            if (!exists) return BadRequest("Invalid PipelineId.");

            var seg = new Segment
            {
                PipelineId = dto.PipelineId,
                Name = dto.Name.Trim(),
                StartLat = dto.StartLat,
                StartLng = dto.StartLng,
                EndLat = dto.EndLat,
                EndLng = dto.EndLng,
                LengthKm = dto.LengthKm
            };

            _db.Segments.Add(seg);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAll), new { pipelineId = seg.PipelineId }, new { seg.SegmentId });
        }

        // PUT: api/segments/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SegmentUpdateDto dto)
        {
            var seg = await _db.Segments.FindAsync(id);
            if (seg is null) return NotFound();

            seg.Name = dto.Name.Trim();
            seg.StartLat = dto.StartLat;
            seg.StartLng = dto.StartLng;
            seg.EndLat = dto.EndLat;
            seg.EndLng = dto.EndLng;
            seg.LengthKm = dto.LengthKm;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/segments/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var seg = await _db.Segments.FindAsync(id);
            if (seg is null) return NotFound();

            _db.Segments.Remove(seg);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
